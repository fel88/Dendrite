using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Dendrite.MergerWindow;

namespace Dendrite
{
    public partial class Processing : Form
    {
        public Processing()
        {
            InitializeComponent();
        }

        string _netPath;
        List<NodeInfo> _nodes = new List<NodeInfo>();
        internal void Init(string lastPath)
        {
            Text = "Processing: " + lastPath;
            _netPath = lastPath;
            var session1 = new InferenceSession(_netPath);
            foreach (var item in session1.OutputMetadata.Keys)
            {
                var dims = session1.OutputMetadata[item].Dimensions;
                _nodes.Add(new NodeInfo() { Name = item, Dims = dims });
                listView1.Items.Add(new ListViewItem(new string[] { item, string.Join("x", session1.OutputMetadata[item].Dimensions), "" }) { Tag = _nodes.Last() });
            }

            foreach (var name in session1.InputMetadata.Keys)
            {
                var dims = session1.InputMetadata[name].Dimensions;
                var s1 = string.Join("x", dims);
                _nodes.Add(new NodeInfo() { Name = name, Dims = dims, IsInput = true });
                listView2.Items.Add(new ListViewItem(new string[] { name, s1, session1.InputMetadata[name].ElementType.Name }) { Tag = _nodes.Last() });
            }
        }

        public class NodeInfo
        {
            public bool IsInput;
            public string Name;
            public int[] Dims;
            public List<string> Tags = new List<string>();
        }

        public Dictionary<string, InputInfo> InputDatas = new Dictionary<string, InputInfo>();
        Dictionary<string, object> OutputDatas = new Dictionary<string, object>();

        float[] inputData;
        private void button1_Click(object sender, EventArgs e)
        {
            run();
        }

        Mat lastReadedMat;
        private void run()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var session1 = new InferenceSession(_netPath);

            var inputMeta = session1.InputMetadata;
            var container = new List<NamedOnnxValue>();

            Mat mat2 = null;
            foreach (var name in inputMeta.Keys)
            {
                var data = InputDatas[name];
                if (data.Data is Mat matOrig)
                {

                    var mat = matOrig.Clone();
                    if (inputMeta[name].Dimensions[2] == -1)
                    {
                        inputMeta[name].Dimensions[2] = mat.Height;
                        inputMeta[name].Dimensions[3] = mat.Width;
                    }

                    mat2 = mat.Clone();
                    mat.ConvertTo(mat, MatType.CV_32F);
                    object param = mat;
                    foreach (var pitem in data.Preprocessors)
                    {
                        param = pitem.Process(param);
                    }

                    inputData = param as float[];
                    var tensor = new DenseTensor<float>(param as float[], inputMeta[name].Dimensions);

                    container.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));
                }
                if (data.Data is VideoCapture cap)
                {
                    Mat mat = new Mat();
                    cap.Read(mat);
                    lastReadedMat = mat.Clone();
                    if (inputMeta[name].Dimensions[2] == -1)
                    {
                        inputMeta[name].Dimensions[2] = mat.Height;
                        inputMeta[name].Dimensions[3] = mat.Width;
                    }
                    pictureBox1.Image = BitmapConverter.ToBitmap(mat);
                    mat2 = mat.Clone();
                    mat.ConvertTo(mat, MatType.CV_32F);
                    object param = mat;
                    foreach (var pitem in data.Preprocessors)
                    {
                        param = pitem.Process(param);
                    }

                    inputData = param as float[];
                    var tensor = new DenseTensor<float>(param as float[], inputMeta[name].Dimensions);

                    container.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));
                }
                if (data.Data is float[] fl)
                {
                    var tensor = new DenseTensor<float>(fl, inputMeta[name].Dimensions);

                    container.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));

                }
            }
            OutputDatas.Clear();
            using (var results = session1.Run(container))
            {

                // Get the results
                foreach (var result in results)
                {
                    var data = result.AsTensor<float>();
                    //var dims = data.Dimensions;
                    var rets = data.ToArray();
                    OutputDatas.Add(result.Name, rets);
                }
            }

            if (checkBox1.Checked)
            {
                Stopwatch sw2 = Stopwatch.StartNew();
                var ret = boxesDecode(mat2);
                sw2.Stop();
                toolStripStatusLabel1.Text = $"decode time: {sw2.ElapsedMilliseconds}ms";
                if (ret != null)
                {
                    var mm = drawBoxes(mat2, ret.Item1, ret.Item2);
                    pictureBox1.Image = BitmapConverter.ToBitmap(mm);
                    mat2 = mm;
                }
            }
            if (vid != null)
            {
                vid.Write(mat2);
            }
            sw.Stop();
            toolStripStatusLabel1.Text = $"{sw.ElapsedMilliseconds}ms";

        }
        string lastPath;
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() != DialogResult.OK) return;
            lastPath = ofd.FileName;

            if (ofd.FileName.EndsWith("mp4") || ofd.FileName.EndsWith("avi") || ofd.FileName.EndsWith("mkv"))
            {
                VideoCapture cap = new VideoCapture(ofd.FileName);
                Mat mat = new Mat();
                cap.Read(mat);


                Text = $"Processing: {ofd.FileName}  {mat.Width}x{mat.Height}";

                if (InputDatas.ContainsKey(currentNode.Name) && InputDatas[currentNode.Name] is InputInfo ii)
                {
                    ii.Data = cap;
                }
                else
                {
                    InputDatas[currentNode.Name] = new InputInfo() { Data = cap };
                }

                pictureBox1.Image = BitmapConverter.ToBitmap(mat);
            }
            else
            {
                var mat = OpenCvSharp.Cv2.ImRead(ofd.FileName);
                Text = $"Processing: {ofd.FileName}  {mat.Width}x{mat.Height}";
                //mat.ConvertTo(mat, MatType.CV_32F);

                if (InputDatas.ContainsKey(currentNode.Name) && InputDatas[currentNode.Name] is InputInfo ii)
                {
                    ii.Data = mat;
                }
                else
                {
                    InputDatas[currentNode.Name] = new InputInfo() { Data = mat };
                }

                pictureBox1.Image = BitmapConverter.ToBitmap(mat);
            }
        }
        NodeInfo currentNode;

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)((listView2.SelectedItems[0] as ListViewItem).Tag);
            if (InputDatas.ContainsKey(currentNode.Name))
            {
                if (InputDatas[currentNode.Name].Data is Mat mat)
                {
                    pictureBox1.Image = BitmapConverter.ToBitmap(mat);
                }
                richTextBox1.Clear();
                if (InputDatas[currentNode.Name].Data is float[] farr)
                {

                    var txt = Form1.GetFormattedArray(new InputData() { Dims = currentNode.Dims.Select(z => (long)z).ToArray(), Weights = farr }, 1000);
                    richTextBox1.Text = txt;
                    listView4.Items.Clear();
                    for (int j = 0; j < 20; j++)
                    {
                        listView4.Items.Add(new ListViewItem(new string[] { j.ToString(), farr[j].ToString() }) { Tag = (long)j });
                    }
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)((listView1.SelectedItems[0] as ListViewItem).Tag);
            if (OutputDatas.ContainsKey(currentNode.Name))
            {
                richTextBox1.Clear();


                if (OutputDatas[currentNode.Name] is float[] farr)
                {
                    var txt = Form1.GetFormattedArray(new InputData() { Dims = currentNode.Dims.Select(z => (long)z).ToArray(), Weights = farr }, 1000);
                    richTextBox1.Text = txt;
                    listView4.Items.Clear();
                    for (int j = 0; j < 20; j++)
                    {
                        listView4.Items.Add(new ListViewItem(new string[] { j.ToString(), farr[j].ToString() }) { Tag = (long)j });
                    }
                }

                if (OutputDatas[currentNode.Name] is Mat mat)
                {
                    pictureBox1.Image = BitmapConverter.ToBitmap(mat);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!(currentPreprocessor is MeanStdPreprocessor mstd)) return;
            try { mstd.Mean[0] = double.Parse(textBox1.Text.Replace(",", "."), CultureInfo.InvariantCulture); }
            catch (Exception ex)
            {
                textBox1.BackColor = Color.Red;
            }
        }

        private void resizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentNode == null) return;
            var r = new ResizePreprocessor() { Dims = currentNode.Dims };
            InputDatas[currentNode.Name].Preprocessors.Add(r);
            listView3.Items.Add(new ListViewItem(new string[] { "resize" }) { Tag = r });
        }

        private void nCHWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentNode == null) return;

            var r = new NCHWPreprocessor();
            InputDatas[currentNode.Name].Preprocessors.Add(r);
            listView3.Items.Add(new ListViewItem(new string[] { "NCHW" }) { Tag = r });
        }

        private void meanstdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentNode == null) return;

            var r = new MeanStdPreprocessor();
            InputDatas[currentNode.Name].Preprocessors.Add(r);
            listView3.Items.Add(new ListViewItem(new string[] { "mean/std" }) { Tag = r });
        }

        private void normalizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentNode == null) return;

            var r = new NormalizePreprocessor();
            InputDatas[currentNode.Name].Preprocessors.Add(r);
            listView3.Items.Add(new ListViewItem(new string[] { "normalize" }) { Tag = r });
        }

        IInputPreprocessor currentPreprocessor;

        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView3.SelectedItems.Count == 0) return;
            var prep = listView3.SelectedItems[0].Tag as IInputPreprocessor;
            currentPreprocessor = prep;
            if (prep is MeanStdPreprocessor mstd)
            {
                textBox1.Text = mstd.Mean[0].ToString();
                textBox2.Text = mstd.Mean[1].ToString();
                textBox3.Text = mstd.Mean[2].ToString();

                textBox4.Text = mstd.Std[0].ToString();
                textBox5.Text = mstd.Std[1].ToString();
                textBox6.Text = mstd.Std[2].ToString();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!(currentPreprocessor is MeanStdPreprocessor mstd)) return;
            try { mstd.Mean[1] = double.Parse(textBox2.Text, CultureInfo.InvariantCulture); }
            catch (Exception ex)
            {
                textBox2.BackColor = Color.Red;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (!(currentPreprocessor is MeanStdPreprocessor mstd)) return;
            try { mstd.Mean[2] = double.Parse(textBox3.Text, CultureInfo.InvariantCulture); }
            catch (Exception ex)
            {
                textBox3.BackColor = Color.Red;
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            if (!(currentPreprocessor is MeanStdPreprocessor mstd)) return;
            try { mstd.Std[0] = double.Parse(textBox6.Text, CultureInfo.InvariantCulture); }
            catch (Exception ex)
            {
                textBox6.BackColor = Color.Red;
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (!(currentPreprocessor is MeanStdPreprocessor mstd)) return;
            try { mstd.Std[1] = double.Parse(textBox5.Text, CultureInfo.InvariantCulture); }
            catch (Exception ex)
            {
                textBox5.BackColor = Color.Red;
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (!(currentPreprocessor is MeanStdPreprocessor mstd)) return;
            try { mstd.Std[2] = double.Parse(textBox4.Text, CultureInfo.InvariantCulture); }
            catch (Exception ex)
            {
                textBox4.BackColor = Color.Red;
            }
        }

        private void template1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentNode == null) return;

            var r = new MeanStdPreprocessor();
            InputDatas[currentNode.Name].Preprocessors.Add(r);
            listView3.Items.Add(new ListViewItem(new string[] { "mean/std" }) { Tag = r });

            var r2 = new ResizePreprocessor() { Dims = currentNode.Dims };
            InputDatas[currentNode.Name].Preprocessors.Add(r2);
            listView3.Items.Add(new ListViewItem(new string[] { "resize" }) { Tag = r2 });

            var r3 = new NormalizePreprocessor();
            InputDatas[currentNode.Name].Preprocessors.Add(r3);
            listView3.Items.Add(new ListViewItem(new string[] { "normalize" }) { Tag = r3 });

            var r4 = new NCHWPreprocessor();
            InputDatas[currentNode.Name].Preprocessors.Add(r4);
            listView3.Items.Add(new ListViewItem(new string[] { "NCHW" }) { Tag = r4 });
        }

        private void convertToIamgeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveImageToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var cc = (NodeInfo)listView1.SelectedItems[0].Tag;
            if (OutputDatas[cc.Name] is Mat dd)
            {
                Clipboard.SetImage(BitmapConverter.ToBitmap(dd));
            }
        }

        private void toFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var cc = (NodeInfo)listView1.SelectedItems[0].Tag;
            if (OutputDatas[cc.Name] is Mat dd)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                if (sfd.ShowDialog() != DialogResult.OK) return;
                dd.SaveImage(sfd.FileName);
            }
        }

        private void binaryMaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var cc = (NodeInfo)listView1.SelectedItems[0].Tag;
            if (OutputDatas[cc.Name] is float[] dd)
            {
                byte[] outp = new byte[cc.Dims[3] * cc.Dims[2]];
                byte b1 = 0;
                byte b2 = 255;

                int shift = cc.Dims[3] * cc.Dims[2];

                for (int i = 0; i < dd.Length / 2; i++)
                {
                    if (dd[i] > dd[i + shift])
                    {
                        outp[i] = b1;
                    }
                    else
                    {
                        outp[i] = b2;
                    }
                }
                Mat mat = new Mat(new int[] { cc.Dims[2], cc.Dims[3] }, MatType.CV_8UC1, outp.ToArray());
                OutputDatas[cc.Name] = mat;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            var txt = Form1.GetFormattedArray(new InputData() { Dims = currentNode.Dims.Select(z => (long)z).ToArray(), Weights = inputData }, 1000);
            richTextBox1.Text = txt;

        }

        private void binaryAsisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var cc = (NodeInfo)listView1.SelectedItems[0].Tag;
            if (OutputDatas[cc.Name] is float[] dd)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                if (sfd.ShowDialog() != DialogResult.OK) return;
                byte[] arrr = new byte[dd.Length * 4];
                for (int i = 0; i < dd.Length; i++)
                {
                    Array.Copy(BitConverter.GetBytes(dd[i]), 0, arrr, i * 4, 4);
                }
                File.WriteAllBytes(sfd.FileName, arrr);

            }
        }

        private void rgbMaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var cc = (NodeInfo)listView1.SelectedItems[0].Tag;
            if (OutputDatas[cc.Name] is float[] dd)
            {
                byte[] outp = new byte[cc.Dims[3] * cc.Dims[2]];

                int shift = cc.Dims[3] * cc.Dims[2];

                for (int i = 0; i < (cc.Dims[3] * cc.Dims[2]); i++)
                {
                    float max = dd[i];
                    int maxk = 0;
                    for (int j = 0; j < cc.Dims[1]; j++)
                    {
                        if (dd[i + shift * j] > max) { max = dd[i + shift * j]; maxk = j; };
                    }

                    outp[i] = (byte)(maxk * 10);

                }
                Mat mat = new Mat(new int[] { cc.Dims[2], cc.Dims[3] }, MatType.CV_8UC1, outp.ToArray());
                OutputDatas[cc.Name] = mat;
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView3.SelectedItems.Count == 0) return;
            var prep = listView3.SelectedItems[0].Tag as IInputPreprocessor;
            if (currentNode == null) return;

            InputDatas[currentNode.Name].Preprocessors.Remove(prep);
            listView3.Items.Remove(listView3.SelectedItems[0]);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void all1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)((listView2.SelectedItems[0] as ListViewItem).Tag);
            int size = 1;
            for (int i = 0; i < currentNode.Dims.Length; i++)
            {
                size *= currentNode.Dims[i];
            }
            var fl = new float[size];
            for (int i = 0; i < fl.Length; i++)
            {
                fl[i] = 1;
            }

            if (InputDatas.ContainsKey(currentNode.Name) && InputDatas[currentNode.Name] is InputInfo ii)
            {

                ii.Data = fl;
            }
            else
            {
                InputDatas[currentNode.Name] = new InputInfo() { Data = fl };
            }


        }

        private void all0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)((listView2.SelectedItems[0] as ListViewItem).Tag);
            int size = 1;
            for (int i = 0; i < currentNode.Dims.Length; i++)
            {
                size *= currentNode.Dims[i];
            }
            var fl = new float[size];
            for (int i = 0; i < fl.Length; i++)
            {
                fl[i] = 0;
            }

            if (InputDatas.ContainsKey(currentNode.Name) && InputDatas[currentNode.Name] is InputInfo ii)
            {

                ii.Data = fl;
            }
            else
            {
                InputDatas[currentNode.Name] = new InputInfo() { Data = fl };
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listView4.SelectedItems.Count == 0) return;
            if (currentNode == null) return;
            var val = long.Parse(textBox9.Text);

            float[] farr;
            if (currentNode.IsInput)
            {
                farr = InputDatas[currentNode.Name].Data as float[];
            }
            else
            {
                farr = OutputDatas[currentNode.Name] as float[];
            }
            var addr = (long)listView4.SelectedItems[0].Tag;
            farr[addr] = val;
            listView4.SelectedItems[0].SubItems[1].Text = val.ToString();

        }
        long offset = 0;
        private void button4_Click(object sender, EventArgs e)
        {
            if (currentNode == null) return;
            offset = long.Parse(textBox9.Text);
            listView4.Items.Clear();
            float[] farr;
            if (currentNode.IsInput)
            {
                farr = InputDatas[currentNode.Name].Data as float[];
            }
            else
            {
                farr = OutputDatas[currentNode.Name] as float[];
            }
            for (int j = 0; j < 20; j++)
            {
                listView4.Items.Add(new ListViewItem(new string[] { (j + offset).ToString("X2"), farr[j + offset].ToString() }) { Tag = (long)(j + offset) });
            }

        }

        private void listView4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void confToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)((listView1.SelectedItems[0] as ListViewItem).Tag);
            if (OutputDatas.ContainsKey(currentNode.Name))
            {
                if (!currentNode.Tags.Contains("conf"))
                {
                    currentNode.Tags.Add("conf");
                    listView1.SelectedItems[0].SubItems[2].Text = string.Join(", ", currentNode.Tags);
                }
            }

        }

        public static Dictionary<string, float[][]> allPriorBoxes = new Dictionary<string, float[][]>();
        private void boxesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Stopwatch sw2 = Stopwatch.StartNew();
            var ret = boxesDecode(InputDatas.First().Value.Data as Mat);
            sw2.Stop();
            toolStripStatusLabel1.Text = $"decode time: {sw2.ElapsedMilliseconds}ms";

            if (ret == null)
            {
                MessageBox.Show("Set conf and loc outputs first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            var mm = drawBoxes(InputDatas.First().Value.Data as Mat, ret.Item1, ret.Item2);
            pictureBox1.Image = BitmapConverter.ToBitmap(mm);
        }
        float visTresh = 0.5f;

        Mat drawBoxes(Mat mat1, Rect[] detections, float[] oscores)
        {

            Mat mat = mat1.Clone();


            for (int i = 0; i < detections.Length; i++)
            {
                if (oscores[i] < visTresh) continue;
                mat.Rectangle(detections[i], new OpenCvSharp.Scalar(255, 0, 0), 2);
                var text = Math.Round(oscores[i], 4).ToString();
                var cx = detections[i].X;
                var cy = detections[i].Y + 12;
                mat.PutText(text, new OpenCvSharp.Point(cx, cy),
                            HersheyFonts.HersheyDuplex, 0.5, new Scalar(255, 255, 255));
            }
            return mat;

        }
        public Tuple<Rect[], float[]> boxesDecode(Mat mat1)
        {
            var f1 = _nodes.FirstOrDefault(z => z.Tags.Contains("conf"));
            var f2 = _nodes.FirstOrDefault(z => z.Tags.Contains("loc"));
            if (f1 == null || f2 == null)
            {
                return null;
            }
            var rets1 = OutputDatas[f2.Name] as float[];
            var rets3 = OutputDatas[f1.Name] as float[];
            var dims = _nodes.First(z => z.IsInput).Dims;
            var sz = new System.Drawing.Size(dims[3], dims[2]);
            if (dims[2] == -1)
            {
                sz.Height = mat1.Height;
                sz.Width = mat1.Width;
            }
            string key = $"{sz.Width}x{sz.Height}";
            if (!allPriorBoxes.ContainsKey(key))
            {
                var pd = Decoders.PriorBoxes2(sz.Width, sz.Height); ;
                allPriorBoxes.Add(key, pd);
            }
            var prior_data = allPriorBoxes[key];
            return boxesDecode(mat1, rets1, rets3, sz, prior_data);

        }

        public static Tuple<Rect[], float[]> boxesDecode(Mat mat1, float[] confd, float[] locd, System.Drawing.Size sz, float[][] prior_data)
        {
            Stopwatch sw = Stopwatch.StartNew();

            if (confd == null || locd == null)
            {
                return null;
            }
            var matSize = (mat1).Size();



            List<float[]> loc = new List<float[]>();
            List<float> scores = new List<float>();

            float[] variances = new float[] { 0.1f, 0.2f };
            var nnInputWidth = sz.Width;
            var nnInputHeight = sz.Height;
            float wz1 = nnInputWidth;
            float hz1 = nnInputHeight;
            float[] scale = new float[] { (float)nnInputWidth, (float)nnInputHeight, (float)nnInputWidth, (float)nnInputHeight };
            float koef = wz1 / (float)(matSize.Width);
            float koef2 = hz1 / (float)(matSize.Height);


            float[] resize = new float[] { koef, koef2 };

            var rets3 = locd;
            var rets1 = confd;


            for (var i = 0; i < rets1.Length; i += 4)
            {
                loc.Add(new float[] { rets1[i + 0], rets1[i + 1], rets1[i + 2], rets1[i + 3] });
            }

            for (var i = 0; i < rets3.Length; i += 2)
            {
                scores.Add(rets3[i + 1]);
            }

            var boxes = Decoders.decode(loc, prior_data, variances);
            for (var i = 0; i < boxes.Count(); i++)
            {
                boxes[i][0] = (boxes[i][0] * scale[0]) / resize[0];
                boxes[i][1] = (boxes[i][1] * scale[1]) / resize[1];
                boxes[i][2] = (boxes[i][2] * scale[2]) / resize[0];
                boxes[i][3] = (boxes[i][3] * scale[3]) / resize[1];
            }

            float[] scale1 = new float[] { wz1, hz1, wz1, hz1, wz1, hz1, wz1, hz1, wz1, hz1 };

            float confidence_threshold = 0.2f;
            List<int> inds = new List<int>();

            for (var i = 0; i < scores.Count(); i++)
            {
                if (scores[i] > confidence_threshold)
                {
                    inds.Add(i);
                }
            }

            List<float[]> boxes2 = new List<float[]>();
            for (var i = 0; i < inds.Count(); i++)
            {
                boxes2.Add(boxes[inds[i]]);
            }
            boxes = boxes2.ToArray();

            List<float> scores2 = new List<float>();
            for (var i = 0; i < inds.Count(); i++)
            {
                scores2.Add(scores[inds[i]]);
            }
            scores = scores2;
            var order = Decoders.sort_indexes(scores);
            List<float[]> boxes3 = new List<float[]>();
            for (var i = 0; i < order.Count(); i++)
            {
                boxes3.Add(boxes[order[i]]);

            }

            boxes = boxes3.ToArray();

            List<float> scores3 = new List<float>();
            for (var i = 0u; i < order.Count(); i++)
            {
                scores3.Add(scores[order[i]]);

            }

            scores = scores3;
            //2. nms
            List<float[]> dets = new List<float[]>();
            for (var i = 0; i < boxes.Count(); i++)
            {
                dets.Add(new float[] { boxes[i][0], boxes[i][1], boxes[i][2], boxes[i][3], scores[i] });
            }
            var keep = Decoders.nms(dets, 0.4f);

            List<float[]> dets2 = new List<float[]>();

            for (var i = 0u; i < keep.Count(); i++)
            {
                dets2.Add(dets[keep[i]]);
            }
            dets = dets2;

            List<Rect> detections = new List<Rect>();

            float vis_thresh = 0.5f;

            List<int> indexMap = new List<int>();

            List<float[]> odets = new List<float[]>();
            List<float> oscores = new List<float>();

            for (var i = 0; i < dets.Count(); i++)
            {
                var aa = dets[i];
                if (aa[4] < vis_thresh) continue;
                detections.Add(new Rect((int)(aa[0]), (int)(aa[1]), (int)(aa[2] - aa[0]), (int)(aa[3] - aa[1])));
                indexMap.Add(i);

                oscores.Add(scores3[i]);
            }

            for (var i = 0; i < dets.Count(); i++)
            {
                odets.Add(dets[i]);
            }
            sw.Stop();

            var ret = new Tuple<Rect[], float[]>(detections.ToArray(), oscores.ToArray());
            return ret;
        }

        private void locationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)((listView1.SelectedItems[0] as ListViewItem).Tag);
            if (OutputDatas.ContainsKey(currentNode.Name))
            {
                if (!currentNode.Tags.Contains("loc"))
                {
                    currentNode.Tags.Add("loc");
                    listView1.SelectedItems[0].SubItems[2].Text = string.Join(", ", currentNode.Tags);
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }
        bool forwTo = false;
        double forwPosPercetange = 0;
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);


            var pc = pictureBox2.PointToClient(Cursor.Position);
            var ff = pc.X / (float)pictureBox2.Width;
            forwTo = true;
            forwPosPercetange = ff;

            gr.FillRectangle(Brushes.LightBlue, 0, 0, (int)(forwPosPercetange * bmp.Width), bmp.Height);
            pictureBox2.Image = bmp;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Enabled = checkBox3.Checked;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (forwTo)
            {
                var cap = InputDatas.First().Value.Data as VideoCapture;
                var ofps = cap.Get(VideoCaptureProperties.Fps);

                forwTo = false;
                var frm = cap.Get(VideoCaptureProperties.FrameCount);
                var secs = (frm / ofps) * 1000;

                cap.Set(VideoCaptureProperties.PosMsec, forwPosPercetange * secs);
            }
            run();
        }
        VideoWriter vid;

        private void button7_Click(object sender, EventArgs e)
        {
            if (vid != null)
            {
                vid.Release();
                vid = null;
                return;
            }
            vid = new VideoWriter("output.mp4", FourCC.XVID, 25, new OpenCvSharp.Size(pictureBox1.Image.Width, pictureBox1.Image.Height));

        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            try
            {
                visTresh = float.Parse(textBox13.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                textBox13.BackColor = Color.White;
            }
            catch (Exception ex)
            {
                textBox13.BackColor = Color.Red;
            }
        }

        List<Mat> savedFrames = new List<Mat>();
        private void button8_Click(object sender, EventArgs e)
        {
            if (lastReadedMat == null) return;
            savedFrames.Add(lastReadedMat);
            Clipboard.SetImage(BitmapConverter.ToBitmap(lastReadedMat));
            listView5.Items.Add(new ListViewItem(new string[] { "frame" }) { });
        }

        private void button9_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK) return;
            int index = 0;
            foreach (var item in savedFrames)
            {
                var fi = new FileInfo(sfd.FileName);
                string path = Path.Combine(fi.DirectoryName, $"{index}_screenshot_" + fi.Name);
                index++;
                item.SaveImage(path);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            StatisticForm s = new StatisticForm();
            var nd = _nodes.First(z => z.IsInput);
            s.Init(lastPath, _netPath, nd.Name, nd.Dims, InputDatas[nd.Name].Preprocessors.ToArray());
        }

        private void button11_Click(object sender, EventArgs e)
        {
            listView5.Items.Clear();
            savedFrames.Clear();

        }
    }

    public interface IInputPreprocessor
    {
        object Process(object input);
    }
    public class InputInfo
    {
        public List<IInputPreprocessor> Preprocessors = new List<IInputPreprocessor>();
        public object Data;
    }

    public class MeanStdPreprocessor : IInputPreprocessor
    {
        public double[] Mean = new double[3];
        public double[] Std = new double[3];

        public object Process(object inp)
        {
            var input = inp as Mat;
            Cv2.Subtract(input, new Scalar(Mean[0], Mean[1], Mean[2]), input);
            var res = Cv2.Split(input);
            for (int i = 0; i < 3; i++)
            {
                res[i] /= Std[i];
            }
            Cv2.Merge(res, input);
            return input;
        }
    }
    public class ResizePreprocessor : IInputPreprocessor
    {

        public int[] Dims;

        public object Process(object inp)
        {
            var input = inp as Mat;
            return input.Resize(new OpenCvSharp.Size(Dims[3], Dims[2]));
        }
    }
    public class NormalizePreprocessor : IInputPreprocessor
    {
        public object Process(object inp)
        {
            var input = inp as Mat;
            input /= 255f;
            return input;
        }
    }
    public class NCHWPreprocessor : IInputPreprocessor
    {
        public object Process(object inp)
        {
            var input = inp as Mat;
            var res2 = input.Split();
            res2[0].GetArray<float>(out float[] ret1);
            res2[1].GetArray<float>(out float[] ret2);
            res2[2].GetArray<float>(out float[] ret3);

            var inputData = new float[ret1.Length + ret2.Length + ret3.Length];

            Array.Copy(ret1, 0, inputData, 0, ret1.Length);
            Array.Copy(ret2, 0, inputData, ret1.Length, ret2.Length);
            Array.Copy(ret3, 0, inputData, ret1.Length + ret2.Length, ret3.Length);
            return inputData;
        }
    }
}
