using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        internal void Init(string lastPath)
        {
            Text = "Processing: " + lastPath;
            _netPath = lastPath;
            var session1 = new InferenceSession(_netPath);
            foreach (var item in session1.OutputMetadata.Keys)
            {
                var dims = session1.OutputMetadata[item].Dimensions;
                listView1.Items.Add(new ListViewItem(new string[] { item, string.Join("x", session1.OutputMetadata[item].Dimensions) }) { Tag = new NodeInfo() { Name = item, Dims = dims } });
            }

            foreach (var name in session1.InputMetadata.Keys)
            {
                var dims = session1.InputMetadata[name].Dimensions;
                var s1 = string.Join("x", dims);
                listView2.Items.Add(new ListViewItem(new string[] { name, s1, session1.InputMetadata[name].ElementType.Name }) { Tag = new NodeInfo() { Name = name, Dims = dims } });
            }
        }

        public class NodeInfo
        {
            public string Name;
            public int[] Dims;
        }

        public Dictionary<string, InputInfo> InputDatas = new Dictionary<string, InputInfo>();
        Dictionary<string, object> OutputDatas = new Dictionary<string, object>();

        float[] inputData;
        private void button1_Click(object sender, EventArgs e)
        {
            var session1 = new InferenceSession(_netPath);

            var inputMeta = session1.InputMetadata;
            var container = new List<NamedOnnxValue>();


            foreach (var name in inputMeta.Keys)
            {
                var data = InputDatas[name];
                if (data.Data is Mat matOrig)
                {
                    var mat = matOrig.Clone();
                    mat.ConvertTo(mat, MatType.CV_32F);
                    object param = mat;
                    foreach (var pitem in data.Preprocessors)
                    {
                        param = pitem.Process(param);
                    }


                    /*mat = mat.Resize(new OpenCvSharp.Size(inputMeta[name].Dimensions[3], inputMeta[name].Dimensions[2]));
                    mat -= 127.5f;
                    mat /= 128f;*/


                    inputData = param as float[];
                    var tensor = new DenseTensor<float>(param as float[], inputMeta[name].Dimensions);

                    container.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));
                }
            }
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
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() != DialogResult.OK) return;

            var mat = OpenCvSharp.Cv2.ImRead(ofd.FileName);
            //mat.ConvertTo(mat, MatType.CV_32F);

            InputDatas[currentInput.Name] = new InputInfo() { Data = mat };
            pictureBox1.Image = BitmapConverter.ToBitmap(mat);
        }
        NodeInfo currentInput;

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            currentInput = (NodeInfo)((listView2.SelectedItems[0] as ListViewItem).Tag);
            if (InputDatas.ContainsKey(currentInput.Name))
            {
                if (InputDatas[currentInput.Name].Data is Mat mat)
                {
                    pictureBox1.Image = BitmapConverter.ToBitmap(mat);
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var currentOuput = (NodeInfo)((listView1.SelectedItems[0] as ListViewItem).Tag);
            if (OutputDatas.ContainsKey(currentOuput.Name))
            {
                richTextBox1.Clear();


                if (OutputDatas[currentOuput.Name] is float[] farr)
                {
                    var txt = Form1.GetFormattedArray(new InputData() { Dims = currentOuput.Dims.Select(z => (long)z).ToArray(), Weights = farr }, 1000);
                    richTextBox1.Text = txt;
                }

                if (OutputDatas[currentOuput.Name] is Mat mat)
                {
                    pictureBox1.Image = BitmapConverter.ToBitmap(mat);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!(currentPreprocessor is MeanStdPreprocessor mstd)) return;
            try { mstd.Mean[0] = double.Parse(textBox1.Text); }
            catch (Exception ex)
            {
                textBox1.BackColor = Color.Red;
            }
        }

        private void resizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentInput == null) return;
            var r = new ResizePreprocessor() { Dims = currentInput.Dims };
            InputDatas[currentInput.Name].Preprocessors.Add(r);
            listView3.Items.Add(new ListViewItem(new string[] { "resize" }) { Tag = r });
        }

        private void nCHWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentInput == null) return;

            var r = new NCHWPreprocessor();
            InputDatas[currentInput.Name].Preprocessors.Add(r);
            listView3.Items.Add(new ListViewItem(new string[] { "NCHW" }) { Tag = r });
        }

        private void meanstdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentInput == null) return;

            var r = new MeanStdPreprocessor();
            InputDatas[currentInput.Name].Preprocessors.Add(r);
            listView3.Items.Add(new ListViewItem(new string[] { "mean/std" }) { Tag = r });
        }

        private void normalizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentInput == null) return;

            var r = new NormalizePreprocessor();
            InputDatas[currentInput.Name].Preprocessors.Add(r);
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
            try { mstd.Mean[1] = double.Parse(textBox2.Text); }
            catch (Exception ex)
            {
                textBox2.BackColor = Color.Red;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (!(currentPreprocessor is MeanStdPreprocessor mstd)) return;
            try { mstd.Mean[2] = double.Parse(textBox3.Text); }
            catch (Exception ex)
            {
                textBox3.BackColor = Color.Red;
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            if (!(currentPreprocessor is MeanStdPreprocessor mstd)) return;
            try { mstd.Std[0] = double.Parse(textBox6.Text); }
            catch (Exception ex)
            {
                textBox6.BackColor = Color.Red;
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (!(currentPreprocessor is MeanStdPreprocessor mstd)) return;
            try { mstd.Std[1] = double.Parse(textBox5.Text); }
            catch (Exception ex)
            {
                textBox5.BackColor = Color.Red;
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (!(currentPreprocessor is MeanStdPreprocessor mstd)) return;
            try { mstd.Std[2] = double.Parse(textBox4.Text); }
            catch (Exception ex)
            {
                textBox4.BackColor = Color.Red;
            }
        }

        private void template1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentInput == null) return;

            var r = new MeanStdPreprocessor();
            InputDatas[currentInput.Name].Preprocessors.Add(r);
            listView3.Items.Add(new ListViewItem(new string[] { "mean/std" }) { Tag = r });

            var r2 = new ResizePreprocessor() { Dims = currentInput.Dims };
            InputDatas[currentInput.Name].Preprocessors.Add(r2);
            listView3.Items.Add(new ListViewItem(new string[] { "resize" }) { Tag = r2 });

            var r3 = new NormalizePreprocessor();
            InputDatas[currentInput.Name].Preprocessors.Add(r3);
            listView3.Items.Add(new ListViewItem(new string[] { "normalize" }) { Tag = r3 });

            var r4 = new NCHWPreprocessor();
            InputDatas[currentInput.Name].Preprocessors.Add(r4);
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
            var txt = Form1.GetFormattedArray(new InputData() { Dims = currentInput.Dims.Select(z => (long)z).ToArray(), Weights = inputData }, 1000);
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
