using Dendrite.Dialogs;
using Dendrite.Lib;
using Dendrite.Preprocessors;
using Dendrite.Preprocessors.Controls;
using Microsoft.ML.OnnxRuntime;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;

namespace Dendrite
{
    public partial class Processing : Form
    {
        public Processing()
        {
            InitializeComponent();
            ctx.Init(pictureBox3);
            pictureBox3.SetDoubleBuffered(true);
            pictureBox3.Paint += PictureBox3_Paint;
            pictureBox3.MouseDown += PictureBox3_MouseDown;
            pictureBox3.MouseUp += PictureBox3_MouseUp;
            Load += Processing_Load;

            tableLayoutPanel1.ColumnStyles[0].Width = 0;
            tableLayoutPanel1.RowStyles[0].Height = 0;
        }

        private void PictureBox3_MouseUp(object sender, MouseEventArgs e)
        {
            if (startedPin != null && hoveredPin != null)
            {
                PinLink link = new PinLink() { Input = startedPin, Output = hoveredPin };
                hoveredPin.InputLinks.Add(link);
                startedPin.OutputLinks.Add(link);
            }
            startedPin = null;
            dragged = null;
        }

        public void SelectedChanged()
        {
            propertyGrid1.SelectedObject = selected.Node.Tag;
            propertyGrid2.SelectedObject = selected.Node;
            propertyGrid3.SelectedObject = selected;
            toolStripStatusLabel1.ForeColor = Color.Black;
            if (selected.Node.LastException != null)
            {
                toolStripStatusLabel1.ForeColor = Color.Red;
                toolStripStatusLabel1.Text = selected.Node.LastException.Message;
            }

            (selected as NodeUI).IsSelected = true;
            UpdateSelectedNodeControl();
            if (selected.Node is NetNode nn)
            {
                UpdateNodesList(nn.Net);
            }
        }

        private void PictureBox3_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (selected != null)
                {
                    (selected as NodeUI).IsSelected = false;
                    selected = null;
                }
                if (hovered != null)
                {
                    if (hoveredPin == null)
                    {
                        dragged = hovered;
                        startDragPositionX = dragged.Position.X;
                        startDragPositionY = dragged.Position.Y;
                        var pos = pictureBox3.PointToClient(Cursor.Position);
                        var pos2 = ctx.BackTransform(pos);
                        startDragX = pos2.X;
                        startDragY = pos2.Y;
                    }
                    else
                    {
                        startedPin = hoveredPin;
                    }
                    selected = hovered;
                    SelectedChanged();


                }
            }
        }
        
        private void UpdateSelectedNodeControl()
        {
            if (!(dragged is NodeUI n)) return;

            if (n.Node.Tag is Nnet net)
            {
                NetControlConfig cc = new NetControlConfig();
                cc.Init(net);
                groupBox1.Controls.Clear();
                cc.Dock = DockStyle.Fill;
                groupBox1.Controls.Add(cc);
                UpdateNodesList(net);
                return;
            }
            if (!(n.Node.Tag is IInputPreprocessor prep)) return;
                        
            groupBox1.Controls.Clear();

            var cands = Assembly.GetCallingAssembly().GetTypes().Where(z => z.GetInterfaces().Contains(typeof(IProcessorConfigControl))).ToArray();

            var ccc = prep.ConfigControl;
            if (ccc == null)
                ccc = cands.FirstOrDefault(z => z.GetCustomAttribute(typeof(PreprocessorBindAttribute)) != null && ((PreprocessorBindAttribute)z.GetCustomAttribute(typeof(PreprocessorBindAttribute))).Type.IsInstanceOfType(prep));

            if (ccc != null)
            {
                var cc = Activator.CreateInstance(ccc) as IProcessorConfigControl;
                cc.Init(prep);
                var cc2 = cc as UserControl;
                cc2.Dock = DockStyle.Fill;
                groupBox1.Controls.Add(cc2);
            }
        }

        private void Processing_Load(object sender, EventArgs e)
        {
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);
        }

        MessageFilter mf = null;

        PipelineUI pipelineUI = new PipelineUI();

        IUIElement dragged;
        NodeUI selected;
        NodeUI hovered;
        NodePin hoveredPin;
        NodePin startedPin;
        float startDragX;
        float startDragY;
        float startDragPositionX;
        float startDragPositionY;
        private void PictureBox3_Paint(object sender, PaintEventArgs e)
        {
            var pos = pictureBox3.PointToClient(Cursor.Position);
            if (dragged != null)
            {
                var back = ctx.BackTransform(pos);
                dragged.Position = new PointF(startDragPositionX + back.X - startDragX, startDragPositionY + back.Y - startDragY);
            }
            UpdateHovered();
            ctx.Graphics = e.Graphics;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.Clear(Color.DarkGray);
            //draw grid
            DrawGrid();
            pipelineUI.Draw(ctx);


        }

        private void UpdateHovered()
        {
            var pos = pictureBox3.PointToClient(Cursor.Position);
            var pos2 = ctx.BackTransform(pos);
            ctx.AllowDrag = dragged == null;
            hovered = null;
            foreach (var item in pipelineUI.Elements.OfType<NodeUI>())
            {
                if (item.ContainsPoint(pos2))
                {
                    hovered = item;
                    ctx.AllowDrag = false;
                    break;
                }
            }
            hoveredPin = null;
            if (hovered != null)
            {
                if ((hovered as NodeUI).HoveredPin != null)
                {
                    hoveredPin = (hovered as NodeUI).HoveredPin;
                }
            }

        }

        private void DrawGrid()
        {
            int cw = 50;
            int ch = 50;
            int sz = 50;
            Pen gridPen = new Pen(Color.Gray, 1f);
            for (int i = -sz; i <= sz; i++)
            {
                for (int j = -sz; j <= sz; j++)
                {
                    float xx = i * cw;
                    float yy = j * ch;
                    var pp = ctx.Transform(xx, yy);
                    xx = pp.X;
                    yy = pp.Y;
                    ctx.Graphics.DrawRectangle(gridPen, xx, yy, cw * ctx.zoom, ch * ctx.zoom);
                }
            }

        }

        InferenceEnvironment env = new InferenceEnvironment();
        Nnet net => env.Net;
        private void button1_Click(object sender, EventArgs e)
        {
            net.Run();
        }

        DrawingContext ctx = new DrawingContext();


        string lastPath;
        private void button2_Click(object sender, EventArgs e)
        { }

        private void LoadImage(NodeInfo node)
        {
            try
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

                    if (InputDatas.ContainsKey(node.Name) && InputDatas[node.Name] is InputInfo ii)
                    {
                        ii.Data = cap;
                    }
                    else
                    {
                        InputDatas[node.Name] = new InputInfo() { Data = cap };
                    }

                    pictureBox1.Image = BitmapConverter.ToBitmap(mat);
                }
                else
                {
                    var mat = OpenCvSharp.Cv2.ImRead(ofd.FileName);
                    Text = $"Processing: {ofd.FileName}  {mat.Width}x{mat.Height}";
                    //mat.ConvertTo(mat, MatType.CV_32F);

                    if (InputDatas.ContainsKey(node.Name) && InputDatas[node.Name] is InputInfo ii)
                    {
                        ii.Data = mat;
                    }
                    else
                    {
                        InputDatas[node.Name] = new InputInfo() { Data = mat };
                    }

                    pictureBox1.Image = BitmapConverter.ToBitmap(mat);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (InputDatas[currentNode.Name].Data is InternalArray iarr)
                {

                    var txt = Form1.GetFormattedArray(new InputData() { Dims = currentNode.Dims.Select(z => (long)z).ToArray(), Weights = iarr.Data.Select(z => (float)z).ToArray() }, 1000);
                    richTextBox1.Text = txt;
                    listView4.Items.Clear();
                    for (int j = 0; j < 20; j++)
                    {
                        listView4.Items.Add(new ListViewItem(new string[] { j.ToString(), iarr.Data[j].ToString() }) { Tag = (long)j });
                    }

                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)(listView1.SelectedItems[0].Tag);
            if (!OutputDatas.ContainsKey(currentNode.Name)) return;

            richTextBox1.Clear();

            if (OutputDatas[currentNode.Name] is float[] farr)
            {
                var txt = Form1.GetFormattedArray(new InputData() { Dims = currentNode.Dims.Select(z => (long)z).ToArray(), Weights = farr }, 1000);
                richTextBox1.Text = txt;
                listView4.Items.Clear();
                for (int j = 0; j < Math.Min(farr.Length, 20); j++)
                {
                    listView4.Items.Add(new ListViewItem(new string[] { j.ToString(), farr[j].ToString() }) { Tag = (long)j });
                }
            }
            if (OutputDatas[currentNode.Name] is byte[] barr)
            {
                richTextBox1.Text = "";
                listView4.Items.Clear();
                for (int j = 0; j < Math.Min(barr.Length, 20); j++)
                {
                    listView4.Items.Add(new ListViewItem(new string[] { j.ToString(), barr[j].ToString() }) { Tag = (long)j });
                }
            }
            if (OutputDatas[currentNode.Name] is Mat mat)
            {
                pictureBox1.Image = BitmapConverter.ToBitmap(mat);
            }
        }

        //private void textBox1_TextChanged(object sender, EventArgs e)
        //{
        //    if (!(currentPreprocessor is MeanStdPreprocessor mstd)) return;
        //    try { mstd.Mean[0] = double.Parse(textBox1.Text.Replace(",", "."), CultureInfo.InvariantCulture); }
        //    catch (Exception ex)
        //    {
        //        textBox1.BackColor = Color.Red;
        //    }
        //}

        private void resizeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        internal async void LoadEnvironmentAsync(string fileName)
        {
            await Task.Run(() => { LoadEnvironment(fileName); });
        }

        internal void LoadEnvironment(string fileName)
        {
            env.Load(fileName);
            pipelineUI = new PipelineUI(env.Pipeline);
            pipelineUI.Restore(fileName);
            //pipelineUI.Init(env.Pipeline);
            Text = "Inference: " + fileName;
            UpdateNodesList(net);
        }


        private void nCHWToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void meanstdToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void normalizeToolStripMenuItem_Click(object sender, EventArgs e)
        {

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


        public Dictionary<string, InputInfo> InputDatas => net.InputDatas;
        public Dictionary<string, object> OutputDatas => net.OutputDatas;
        private void all1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)((listView2.SelectedItems[0] as ListViewItem).Tag);
            if (currentNode.Dims.Any(z => z < 0))
            {
                if (InputDatas.ContainsKey(currentNode.Name) && InputDatas[currentNode.Name] is InputInfo ii && ii.Data is Mat mt)
                {
                    Mat zmt = new Mat(mt.Height, mt.Width, mt.Type(), new Scalar(1));
                    InputDatas[currentNode.Name] = new InputInfo() { Data = zmt };
                }
                else
                {
                    //set sizes                    
                    ShapeSizeDialog ss = new ShapeSizeDialog();
                    ss.Init(currentNode);
                    ss.ShowDialog();
                    if (currentNode.Dims.All(z => z > 0))
                        fillData(1, currentNode);
                }
            }
            else
            {
                fillData(1, currentNode);
            }
        }

        private void fillData(int v, NodeInfo currentNode, bool outputAsInternalArray = true)
        {
            int size = 1;
            for (int i = 0; i < currentNode.Dims.Length; i++)
            {
                size *= currentNode.Dims[i];
            }
            var fl = new float[size];
            for (int i = 0; i < fl.Length; i++)
            {
                fl[i] = v;
            }

            if (InputDatas.ContainsKey(currentNode.Name) && InputDatas[currentNode.Name] is InputInfo ii)
            {
                if (outputAsInternalArray)
                {
                    var intar = new InternalArray(currentNode.Dims);
                    intar.Data = fl.Select(z => (double)z).ToArray();
                    ii.Data = intar;
                }
                else
                    ii.Data = fl;
            }
            else
            {
                if (outputAsInternalArray)
                {
                    var intar = new InternalArray(currentNode.Dims);
                    intar.Data = fl.Select(z => (double)z).ToArray();
                    InputDatas[currentNode.Name] = new InputInfo() { Data = intar };
                }
                else
                {
                    InputDatas[currentNode.Name] = new InputInfo() { Data = fl };
                }
            }
        }

        private void all0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)(listView2.SelectedItems[0].Tag);
            if (currentNode.Dims.Any(z => z < 0))
            {
                if (InputDatas.ContainsKey(currentNode.Name) && InputDatas[currentNode.Name] is InputInfo ii && ii.Data is Mat mt)
                {
                    Mat zmt = new Mat(mt.Height, mt.Width, mt.Type(), new Scalar(1));
                    InputDatas[currentNode.Name] = new InputInfo() { Data = zmt };
                }
                else
                {
                    //set sizes                    
                    ShapeSizeDialog ss = new ShapeSizeDialog();
                    ss.Init(currentNode);
                    ss.ShowDialog();
                    if (currentNode.Dims.All(z => z > 0))
                        fillData(0, currentNode);
                }
            }
            else
            {
                fillData(0, currentNode);
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
            /*  if (listView1.SelectedItems.Count == 0) return;
              currentNode = (NodeInfo)((listView1.SelectedItems[0] as ListViewItem).Tag);
              if (OutputDatas.ContainsKey(currentNode.Name))
              {
                  if (!currentNode.Tags.Contains("conf"))
                  {
                      currentNode.Tags.Add("conf");
                      listView1.SelectedItems[0].SubItems[2].Text = string.Join(", ", currentNode.Tags);
                  }
              }
            */
        }

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
            var mm = Helpers.DrawBoxes(InputDatas.First().Value.Data as Mat, ret.Item1, ret.Item2, visTresh, ret.Item3);
            pictureBox1.Image = BitmapConverter.ToBitmap(mm);
        }
        float visTresh = 0.5f;


        public Tuple<Rect[], float[], int[]> boxesDecode(Mat mat1)
        {
            var f1 = net.Nodes.FirstOrDefault(z => z.Tags.Contains("conf"));
            var f2 = net.Nodes.FirstOrDefault(z => z.Tags.Contains("loc"));
            if (f1 == null || f2 == null)
            {
                return null;
            }
            var rets1 = OutputDatas[f2.Name] as float[];
            var rets3 = OutputDatas[f1.Name] as float[];
            var dims = net.Nodes.First(z => z.IsInput).Dims;
            var sz = new System.Drawing.Size(dims[3], dims[2]);
            if (dims[2] == -1)
            {
                sz.Height = mat1.Height;
                sz.Width = mat1.Width;
            }
            string key = $"{sz.Width}x{sz.Height}";
            if (!Decoders.allPriorBoxes.ContainsKey(key))
            {
                var pd = Decoders.PriorBoxes2(sz.Width, sz.Height); ;
                Decoders.allPriorBoxes.Add(key, pd);
            }
            var prior_data = Decoders.allPriorBoxes[key];
            return Decoders.BoxesDecode(mat1.Size(), rets3, rets1, sz, prior_data, visTresh);

        }

        internal void Init(string path)
        {
            if (path.EndsWith(".den"))
            {
                LoadEnvironment(path);
                return;
            }

            net.Init(DiskFilesystem, path);

            Text = "Inference: " + path;
            UpdateNodesList(net);
        }


        void UpdateNodesList(Nnet net)
        {
            listView1.Items.Clear();
            listView2.Items.Clear();

            foreach (var item in net.Nodes.Where(z => !z.IsInput))
            {
                var dims = item.Dims;
                listView1.Items.Add(new ListViewItem(new string[] { item.Name, string.Join("x", dims), "" }) { Tag = item });
            }

            foreach (var item in net.Nodes.Where(z => z.IsInput))
            {
                var dims = item.Dims;
                var s1 = string.Join("x", dims);
                listView2.Items.Add(new ListViewItem(new string[] { item.Name, s1, item.ElementType.Name }) { Tag = item });
            }
        }

        private void locationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*if (listView1.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)((listView1.SelectedItems[0] as ListViewItem).Tag);
            if (OutputDatas.ContainsKey(currentNode.Name))
            {
                if (!currentNode.Tags.Contains("loc"))
                {
                    currentNode.Tags.Add("loc");
                    listView1.SelectedItems[0].SubItems[2].Text = string.Join(", ", currentNode.Tags);
                }
            }*/
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
            rewindVideo(false);


            if (InputDatas.Count == 0) return;
            if (!(InputDatas.First().Value.Data is VideoCapture cap)) return;

            Mat mat = new Mat();
            cap.Read(mat);
            pictureBox1.Image = BitmapConverter.ToBitmap(mat);
        }

        bool exit = false;
        bool pause = false;
        AutoResetEvent pauseEvent = new AutoResetEvent(false);
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

            if (th != null) { exit = true; pause = false; pauseEvent.Set(); return; }
            th = new Thread((() =>
            {
                int cntr = 0;
                InferenceSession session = new InferenceSession(net.NetPath);

                bool b = false;
                while (true)
                {
                    if (pause)
                        pauseEvent.WaitOne();

                    if (oneFrameStep && oneFrameStepDir == -1)
                    {
                        var cap = InputDatas.First().Value.Data as VideoCapture;
                        var pf = cap.Get(1);//1 posframes
                        cap.Set(1, Math.Max(0, pf - 2));
                    }
                    if (oneFrameStep) { pause = true; oneFrameStep = false; }

                    if (exit) { th = null; exit = false; break; }
                    rewindVideo();
                    if (b || disableVideoProcessing)
                    {
                        var cap = InputDatas.First().Value.Data as VideoCapture;
                        Mat mat = new Mat();
                        cap.Read(mat);
                        pictureBox1.Invoke((Action)(() =>
                        {
                            pictureBox1.Image = BitmapConverter.ToBitmap(mat);
                        }));



                        continue;
                    }
                    try
                    {
                        net.Run(session);
                        if (vid != null)
                        {
                            /*var fr = (net.Postprocessors.FirstOrDefault(z => z is IPostDrawer));
                            if (fr != null)
                            {
                                var lm = (fr as IPostDrawer).LastMat;

                                if (vid.FrameSize.Width != lm.Width)
                                {
                                    vid = new VideoWriter("output.mp4", FourCC.XVID, OutputVideoFps, new OpenCvSharp.Size(lm.Width, lm.Height));
                                }

                                vid.Write(lm);
                            }
                            else
                            {
                                vid.Write(net.lastReadedMat);
                            }*/
                        }
                        cntr++;
                        toolStripStatusLabel1.Text = "processed frames: " + cntr;
                    }
                    catch (PrepareDataException ex)
                    {

                        if (ex.IsVideo)
                        {
                            statusStrip1.Invoke((Action)(() =>
                            {
                                toolStripStatusLabel1.Text = "net procces error detected. processing will be continued without nnet processing..";
                                toolStripStatusLabel1.BackColor = Color.Red;
                                toolStripStatusLabel1.ForeColor = Color.White;
                                pictureBox1.Image = BitmapConverter.ToBitmap(ex.SourceMat);
                            }));

                            b = true;

                        }
                    }
                    catch (Exception ex)
                    {
                        statusStrip1.Invoke((Action)(() =>
                        {
                            toolStripStatusLabel1.Text = ex.Message;
                            toolStripStatusLabel1.BackColor = Color.Red;
                            toolStripStatusLabel1.ForeColor = Color.White;
                        }));

                        th = null; exit = false; break;
                    }

                }

            }));
            th.IsBackground = true;
            th.Start();
            return;

            timer1.Enabled = checkBox3.Checked;
        }


        void rewindVideo(bool resetForwTo = true)
        {
            if (!forwTo) return;
            if (InputDatas.Count == 0) return;
            if (!(InputDatas.First().Value.Data is VideoCapture cap)) return;


            var ofps = cap.Get(VideoCaptureProperties.Fps);
            if (resetForwTo)
                forwTo = false;
            var frm = cap.Get(VideoCaptureProperties.FrameCount);
            var secs = (frm / ofps) * 1000;
            cap.Set(VideoCaptureProperties.PosMsec, forwPosPercetange * secs);

        }

        Thread th;
        private void timer1_Tick(object sender, EventArgs e)
        {
            return;
            try
            {
                rewindVideo();
                net.Run();
                if (vid != null)
                {
                    /*var fr = (net.Postprocessors.FirstOrDefault(z => z is IPostDrawer));
                    if (fr != null)
                    {
                        vid.Write((fr as IPostDrawer).LastMat);
                    }
                    else
                    {
                        vid.Write(net.lastReadedMat);
                    }*/
                }
            }
            catch (Exception ex)
            {
                Debugger.Launch();
            }
        }
        VideoWriter vid;

        private void button7_Click(object sender, EventArgs e)
        {

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
            var nd = net.Nodes.First(z => z.IsInput);
            //s.Init(lastPath, net.NetPath, nd.Name, nd.Dims, InputDatas[nd.Name].Preprocessors.ToArray());
            s.ShowDialog();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            listView5.Items.Clear();
            savedFrames.Clear();

        }



        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void aspectResizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentNode == null) return;
            var r = new AspectResizePreprocessor() { Dims = currentNode.Dims };
            if (!InputDatas.ContainsKey(currentNode.Name)) return;
            //InputDatas[currentNode.Name].Preprocessors.Add(r);
            //listView3.Items.Add(new ListViewItem(new string[] { "aspect" }) { Tag = r });
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            RunAsync();
        }

        async void RunAsync()
        {
            await Task.Run(() => { Run(); });
        }
        void Run()
        {
            toolStripButton1.Enabled = false;
            try
            {
                rewindVideo();
                var sw = Stopwatch.StartNew();
                env.Process();
                sw.Stop();
                toolStripStatusLabel1.Text = $"inference time: {sw.ElapsedMilliseconds}ms";
            }
            finally
            {
                toolStripButton1.Enabled = true;
            }
            
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public int OutputVideoFps = 25;
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (vid != null)
            {
                vid.Release();
                toolStripButton2.Text = "record video";

                vid = null;
                return;
            }

            vid = new VideoWriter("output.mp4", FourCC.XVID, OutputVideoFps, new OpenCvSharp.Size(pictureBox1.Image.Width, pictureBox1.Image.Height));
            toolStripButton2.Text = "stop video";

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (net.lastReadedMat == null) return;
            savedFrames.Add(net.lastReadedMat);
            Clipboard.SetImage(BitmapConverter.ToBitmap(net.lastReadedMat));
            listView5.Items.Add(new ListViewItem(new string[] { "frame" }) { });
        }

        private void setCustomInputDataToolStripMenuItem_Click(object sender, EventArgs e)
        {


        }

        private void staticImageToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void loadNpyToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void textToolStripMenuItem_Click(object sender, EventArgs e)
        {
            return;
            if (listView1.SelectedItems.Count == 0) return;
            var cc = (NodeInfo)listView1.SelectedItems[0].Tag;
            if (OutputDatas.ContainsKey(cc.Name) && OutputDatas[cc.Name] is float[] dd)
            {
                var dims = cc.Dims.ToArray();
                if (cc.Dims.Length == 3)
                {
                    var l = dims.ToList();
                    l.Insert(0, 1);
                    dims = l.ToArray();
                }
                InternalArray ar = new InternalArray(dims);
                List<int> maxes = new List<int>();

                int offset = 5835;//depend on alphabet
                var cnt = dd.Length / offset;
                int pos = 0;
                for (int j = 0; j < cnt; j++)
                {
                    int maxi = 0;
                    float maxv = dd[pos];
                    for (int i = 0; i < dims.Last(); i++)
                    {
                        if (dd[pos] > maxv) { maxi = i; maxv = dd[pos]; }
                        pos++;
                    }
                    maxes.Add(maxi);
                }
                //OpenFileDialog ofd = new OpenFileDialog();
                //if (ofd.ShowDialog() != DialogResult.OK)
                //{
                //    var ld = NpyLoader.Load(ofd.FileName);
                //}
                string alphabet = "  !\"#$%&'()*+,-./";
                for (char ch = '0'; ch <= '9'; ch++)
                {
                    alphabet += ch;
                }
                alphabet += ":;<=>?@";
                for (char ch = 'A'; ch <= 'Z'; ch++)
                {
                    alphabet += ch;
                }
                alphabet += "[\\]^ `";
                for (char ch = 'a'; ch <= 'z'; ch++)
                {
                    alphabet += ch;
                }
                string ret = "";
                for (int i = 0; i < maxes.Count; i++)
                {
                    if (maxes[i] == 0) continue;
                    if (alphabet.Length <= maxes[i])
                    {
                        ret += "?"; continue;
                    }
                    ret += alphabet[maxes[i]];
                }

                toolStripStatusLabel1.Text = "decoded text: " + ret;

            }
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void compareWithNumpyToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void showPostprocessDataToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            var txt = Form1.GetFormattedArray(new InputData() { Dims = currentNode.Dims.Select(z => (long)z).ToArray(), Weights = net.inputData }, 1000);
            richTextBox1.Text = txt;
            ArrayComparer ar = new ArrayComparer();
            ar.Init(net.inputData, null);
            ar.ShowDialog();
        }

        private void compareWithNpyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var np = NpyLoader.Load(ofd.FileName);
            ArrayComparer ac = new ArrayComparer();
            ac.Init(np.Data.Select(z => (float)z).ToArray(), net.inputData);
            ac.ShowDialog();
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var session1 = new InferenceSession(net.NetPath);
            var container = new List<NamedOnnxValue>();
            net.prepareData(container, session1);

        }

        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;

            var nd = (NodeInfo)((listView2.SelectedItems[0] as ListViewItem).Tag);
            LoadImage(nd);
        }

        private void nyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;

            var nd = (NodeInfo)((listView2.SelectedItems[0] as ListViewItem).Tag);
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "npy files (*.npy)|*.npy";
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var b = NpyLoader.Load(ofd.FileName);

            InputDatas[nd.Name] = new InputInfo() { Data = b };
        }

        private void heatmapToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var f1 = net.Nodes.FirstOrDefault(z => z.Tags.Contains("heatmap"));

            if (f1 == null)
            {
                return;
            }

            var rets3 = OutputDatas[f1.Name] as float[];
            InternalArray arr = new InternalArray(f1.Dims);
            arr.Data = rets3.Select(z => (double)z).ToArray();
            List<double> score_text = new List<double>();
            List<double> score_link = new List<double>();
            for (int i = 0; i < arr.Data.Length; i += 2)
            {
                score_text.Add(arr.Data[i]);
                score_link.Add(arr.Data[i + 1]);
            }

            Mat mat = new Mat(f1.Dims[1], f1.Dims[2], MatType.CV_8UC1, score_text.Select(z => (byte)(Math.Min((int)(z * 255), 255))).ToArray());

            Cv2.ApplyColorMap(mat, mat, ColormapTypes.Jet);
            OutputDatas[f1.Name] = mat;

        }

        private void yToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)((listView1.SelectedItems[0] as ListViewItem).Tag);
            if (OutputDatas.ContainsKey(currentNode.Name))
            {
                if (!currentNode.Tags.Contains("heatmap"))
                {
                    currentNode.Tags.Add("heatmap");
                    listView1.SelectedItems[0].SubItems[2].Text = string.Join(", ", currentNode.Tags);
                }
            }
        }

        InternalArray softmax(InternalArray ar)
        {
            InternalArray ret = ar.Clone();

            if (ar.Shape.Length == 3)
            {
                List<double> exps = new List<double>();
                for (int i = 0; i < ret.Shape[0]; i++)
                {
                    for (int j = 0; j < ret.Shape[1]; j++)
                    {
                        var tt = ret.Get1DImageFrom3DArray(i, j);
                        var sum = tt.Data.Sum(z => Math.Exp(z));
                        exps.Add(sum);
                    }
                }
                for (int i = 0; i < ret.Shape[0]; i++)
                {
                    for (int j = 0; j < ret.Shape[1]; j++)
                    {
                        for (int k = 0; k < ret.Shape[2]; k++)
                        {
                            var val = Math.Exp(ret.Get3D(i, j, k));
                            val /= exps[i * ret.Shape[1] + j];
                            ret.Set3D(i, j, k, val);
                        }
                    }
                }

            }
            else throw new NotImplementedException();
            return ret;
        }

        InternalArray sum(InternalArray ar)
        {
            InternalArray ret = new InternalArray(new int[] { ar.Shape[1] });
            if (ar.Shape.Length == 3)
            {
                for (int i = 0; i < ar.Shape[0]; i++)
                {
                    for (int j = 0; j < ar.Shape[1]; j++)
                    {
                        var tt = ar.Get1DImageFrom3DArray(i, j);
                        var sum = tt.Data.Sum(z => z);
                        ret.Data[j] = sum;
                    }
                }
            }
            else throw new NotImplementedException();
            return ret;
        }
        private void greedyToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (listView1.SelectedItems.Count == 0) return;
            var cc = (NodeInfo)listView1.SelectedItems[0].Tag;
            if (OutputDatas.ContainsKey(cc.Name) && OutputDatas[cc.Name] is float[] dd)
            {
                InternalArray ar1 = new InternalArray(cc.Dims);
                ar1.Data = dd.Select(z => (double)z).ToArray();
                ar1.Shape[1] = ar1.Data.Length / 187;
                var alphabet = NpyLoader.LoadAsUnicodeArray(File.ReadAllBytes("chars.npy"));
                ar1 = softmax(ar1);
                var temp1 = sum(ar1);
                int[] ignored = new int[] { 183, 180, 176, 185, 179, 97, 100, 186, 103, 98, 178, 105, 175, 172, 104, 184, 181, 182, 173, 102, 106, 99, 174, 177, 101 };
                ar1 = filterIgnored(ar1, ignored);
                var sums = sum(ar1);
                ar1 = div(ar1, sums);
                var maxes = max(ar1);


                //string alphabet = "  !\"#$%&'()*+,-./";
                //for (char ch = '0'; ch <= '9'; ch++)
                //{
                //    alphabet += ch;
                //}
                //alphabet += ":;<=>?@";
                //for (char ch = 'A'; ch <= 'Z'; ch++)
                //{
                //    alphabet += ch;
                //}
                //alphabet += "[\\]^ `";
                //for (char ch = 'a'; ch <= 'z'; ch++)
                //{
                //    alphabet += ch;
                //}
                string ret = "";
                for (int i = 0; i < maxes.Data.Length; i++)
                {
                    if (maxes.Data[i] == 0) continue;
                    if (alphabet.Length <= maxes.Data[i])
                    {
                        ret += "?"; continue;
                    }
                    if (i > 0 && maxes.Data[i - 1] == maxes.Data[i]) continue;
                    ret += alphabet[(int)maxes.Data[i]];
                }

                toolStripStatusLabel1.Text = "decoded text: " + ret;
            }
        }

        private InternalArray max(InternalArray ar)
        {
            InternalArray ret = new InternalArray(new int[] { ar.Shape[1] });
            if (ar.Shape.Length == 3)
            {
                for (int i = 0; i < ar.Shape[0]; i++)
                {
                    for (int j = 0; j < ar.Shape[1]; j++)
                    {
                        var tt = ar.Get1DImageFrom3DArray(i, j);
                        int maxi = 0;
                        double maxv = tt.Data[0];
                        for (int ii = 0; ii < tt.Data.Length; ii++)
                        {
                            if (tt.Data[ii] > maxv)
                            {
                                maxi = ii;
                                maxv = tt.Data[ii];
                            }
                        }
                        var sum = maxi;
                        ret.Data[j] = sum;
                    }
                }
            }
            else throw new NotImplementedException();
            return ret;
        }

        private InternalArray div(InternalArray ar1, InternalArray sums)
        {
            InternalArray ret = ar1.Clone();
            for (int i = 0; i < ret.Shape[0]; i++)
            {
                for (int j = 0; j < ret.Shape[1]; j++)
                {
                    for (int k = 0; k < ret.Shape[2]; k++)
                    {
                        var r = ret.Get3D(i, j, k) / sums.Data[j];
                        ret.Set3D(i, j, k, r);
                    }
                }
            }
            return ret;
        }

        private InternalArray filterIgnored(InternalArray ar1, int[] ignored)
        {
            var ret = ar1.Clone();
            for (int i = 0; i < ar1.Shape[0]; i++)
            {
                for (int j = 0; j < ar1.Shape[1]; j++)
                {
                    for (int k = 0; k < ar1.Shape[2]; k++)
                    {
                        var val = ar1.Get3D(i, j, k);
                        if (ignored.Contains(k))
                        {
                            val = 0;
                        }
                        ret.Set3D(i, j, k, val);
                    }
                }
            }
            return ret;
        }

        private void yoloToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stopwatch sw2 = Stopwatch.StartNew();
            int w = 0, h = 0;
            var mat1 = net.lastReadedMat;
            if (InputDatas.First().Value.Data is Mat m)
            {
                mat1 = m;
                w = m.Width;
                h = m.Height;
            }
            if (InputDatas.First().Value.Data is VideoCapture cap)
            {

                w = cap.FrameWidth;
                h = cap.FrameHeight;
            }
            var ret = YoloDecodePreprocessor.yoloBoxesDecode(net, w, h, 0.8f, 0.4f);
            sw2.Stop();
            toolStripStatusLabel1.Text = $"decode time: {sw2.ElapsedMilliseconds}ms";

            if (ret != null)
            {
                var mm = Helpers.DrawBoxes(mat1, ret, visTresh);
                pictureBox1.Image = BitmapConverter.ToBitmap(mm);
            }
        }

        private void bgr2rgbToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }


        private void yoloDecodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
          
        }


        private void drawBoxesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void templateYoloToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentNode == null) return;

            var r = new ResizePreprocessor() { Dims = currentNode.Dims };
            //InputDatas[currentNode.Name].Preprocessors.Add(r);
            //  listView3.Items.Add(new ListViewItem(new string[] { "resize" }) { Tag = r });

            var r2 = new BGR2RGBPreprocessor() { };
            // InputDatas[currentNode.Name].Preprocessors.Add(r2);
            // listView3.Items.Add(new ListViewItem(new string[] { "bgr2rgb" }) { Tag = r2 });

            var r3 = new NormalizePreprocessor();
            // InputDatas[currentNode.Name].Preprocessors.Add(r3);
            //listView3.Items.Add(new ListViewItem(new string[] { "normalize" }) { Tag = r3 });

            var r4 = new NCHWPreprocessor();
            //InputDatas[currentNode.Name].Preprocessors.Add(r4);
            // listView3.Items.Add(new ListViewItem(new string[] { "NCHW" }) { Tag = r4 });
        }

        private void toRGBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentNode == null) return;
            if (!InputDatas.ContainsKey(currentNode.Name)) return;
            var r = new ToRGBPreprocessor();
            //InputDatas[currentNode.Name].Preprocessors.Add(r);
            //listView3.Items.Add(new ListViewItem(new string[] { "rgb" }) { Tag = r });
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OutputVideoFps = int.Parse(textBox1.Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void keypointDecodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void drawKeypointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = new DrawKeypointsPostProcessor();

            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);

            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (pause)
            {
                pause = false;
                pauseEvent.Set();
            }
            else
            {
                pause = true;
            }
        }
        bool disableVideoProcessing = false;
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            disableVideoProcessing = checkBox4.Checked;
        }
        bool oneFrameStep = false;
        int oneFrameStepDir = 1;
        private void button2_Click_1(object sender, EventArgs e)
        {
            pause = false;
            pauseEvent.Set();
            oneFrameStep = true;
            oneFrameStepDir = 1;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            pause = false;
            pauseEvent.Set();

            oneFrameStep = true;
            oneFrameStepDir = -1;
        }

        private void instanceSegmentationDecoderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = new InstanceSegmentationDecodePreprocessor();


            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);

            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
            //listView6.Items.Add(new ListViewItem(new string[] { "instance segmentation decoder" }) { Tag = r });
        }

        private void instanceSegmentationDrawerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = new DrawInstanceSegmentationPostProcessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
            //listView6.Items.Add(new ListViewItem(new string[] { "instance segmentation drawer" }) { Tag = r });
        }

        private void nmsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            net.FetchNextFrame = checkBox5.Checked;
        }

        private void depthmsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var f1 = net.Nodes.FirstOrDefault(z => z.Tags.Contains("depthmap"));

            if (f1 == null)
            {
                return;
            }

            var rets3 = OutputDatas[f1.Name] as float[];
            InternalArray arr = new InternalArray(f1.Dims);
            arr.Data = rets3.Select(z => (double)z).ToArray();


            Mat mat = new Mat(f1.Dims[2],
                f1.Dims[3], MatType.CV_8UC1,
                arr.Data.Select(z => (byte)(z * 255)).ToArray());

            Cv2.ApplyColorMap(mat, mat, ColormapTypes.Magma);
            OutputDatas[f1.Name] = mat;
        }

        private void depthmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)((listView1.SelectedItems[0] as ListViewItem).Tag);
            if (OutputDatas.ContainsKey(currentNode.Name))
            {
                if (!currentNode.Tags.Contains("depthmap"))
                {
                    currentNode.Tags.Add("depthmap");
                    listView1.SelectedItems[0].SubItems[2].Text
                        = string.Join(", ", currentNode.Tags);
                }
            }
        }

        private void depthmapDecodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void rGBToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void byteArrayasisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var cc = (NodeInfo)listView1.SelectedItems[0].Tag;
            if (!(OutputDatas[cc.Name] is byte[] dd)) return;

            Mat mat = null;
            if (cc.Dims.Last() == 3)
            {
                mat = new Mat(new int[] { cc.Dims[0], cc.Dims[1] }, MatType.CV_8UC3, dd.ToArray());
            }
            else
            {
                mat = new Mat(new int[] { cc.Dims[1], cc.Dims[2] }, MatType.CV_8UC3, dd.ToArray());
            }
            mat = mat.CvtColor(ColorConversionCodes.RGB2BGR);
            OutputDatas[cc.Name] = mat;

        }

        private void nCWHToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public void ShowArray(Array bb)
        {
            listView4.Items.Clear();

            for (int j = 0; j < 20; j++)
            {
                var val = bb.GetValue(j);
                listView4.Items.Add(new ListViewItem(new string[] { j.ToString(), val.ToString() }) { Tag = (long)j });
            }
        }

        private void updateAnDShowRawToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var vv = InputDatas.First().Value;
            var mat = vv.Data as Mat;
            object param = mat;
            /*foreach (var pitem in vv.Preprocessors)
            {
                param = pitem.Process(param);
            }*/

            ShowArray(param as Array);
        }

        private void transposeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentNode == null) return;

            var r = new TransposePreprocessor();
            //  InputDatas[currentNode.Name].Preprocessors.Add(r);
            //listView3.Items.Add(new ListViewItem(new string[] { "transpose" }) { Tag = r });
        }

        private void rgb2bgrToolStripMenuItem_Click(object sender, EventArgs e)
        {
            

        }

        private void numpyFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var cc = (NodeInfo)listView1.SelectedItems[0].Tag;
            if (!(OutputDatas.ContainsKey(cc.Name) && OutputDatas[cc.Name] is float[] dd)) return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "npy files (*.npy)|*.npy";
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var npy = NpyLoader.Load(ofd.FileName);
            int[] dims = new int[cc.Dims.Length];
            for (int i = 0; i < dims.Length; i++)
            {
                if (cc.Dims[i] == -1)
                {
                    dims[i] = npy.Shape[i];
                }
                else
                {
                    dims[i] = cc.Dims[i];
                    if (dims[i] != npy.Shape[i])
                    {
                        Extensions.ShowError($"size mismatch: ({string.Join(",", cc.Dims.ToArray())}) and ({string.Join(",", npy.Shape.ToArray())})", Text);
                        return;
                    }
                }
            }

            var arr1 = new InternalArray(dims);
            arr1.Data = dd.Select(z => (double)z).ToArray();
            if (dd.Length != npy.Data.Length)
            {
                Extensions.ShowError($"size mismatch: ({string.Join(",", arr1.Shape.ToArray())}) and ({string.Join(",", npy.Shape.ToArray())})", Text);
                return;
            }
            float eps = 10e-5f;
            double maxDiff = 0;



            for (int i = 0; i < dd.Length; i++)
            {
                maxDiff = Math.Max(Math.Abs(dd[i] - npy.Data[i]), maxDiff);
                if (Math.Abs(dd[i] - npy.Data[i]) > eps)
                {
                    Extensions.ShowError("value mismatch", Text);
                    ArrayComparer arc = new ArrayComparer();
                    arc.Init(dd, npy.Data.Select(z => (float)z).ToArray());
                    arc.ShowDialog();
                    return;
                }
            }
            Extensions.ShowInfo($"tensors are equal. maxDiff: {maxDiff}", Text);
        }

        private void binaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var cc = (NodeInfo)listView1.SelectedItems[0].Tag;
            if (!(OutputDatas.ContainsKey(cc.Name) && OutputDatas[cc.Name] is float[] dd)) return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files (*.*)|*.*";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            var bb = File.ReadAllBytes(ofd.FileName);

            var cnt = bb.Length / 4;
            var ar1 = new InternalArray(new int[] { 1 });
            List<float> ff = new List<float>();
            for (int i = 0; i < bb.Length; i += 4)
            {
                ff.Add(BitConverter.ToSingle(bb, i));
            }

            if (ff.Count != dd.Length)
            {
                Extensions.ShowError($"size mismatch: ({string.Join(",", ff.Count)}) and ({string.Join(",", dd.Length)})", Text);
                return;
            }


            float eps = 10e-5f;
            double maxDiff = 0;
            for (int i = 0; i < ff.Count; i++)
            {
                maxDiff = Math.Abs(ff[i] - dd[i]);
                if (Math.Abs(ff[i] - dd[i]) > eps)
                {
                    Extensions.ShowError("value mismatch", Text);
                    ArrayComparer arc = new ArrayComparer();
                    arc.Init(dd, ff.ToArray());
                    arc.ShowDialog();
                    return;
                }
            }
            Extensions.ShowInfo($"tensors are equal. maxDiff: {maxDiff}", Text);

        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var cc = (NodeInfo)listView1.SelectedItems[0].Tag;
            if (!(OutputDatas[cc.Name] is float[] dd)) return;

            MessageBox.Show($"size: {dd.Length}", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void randomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)(listView2.SelectedItems[0].Tag);
            bool good = true;
            if (currentNode.Dims.Any(z => z < 1))
            {
                good = false;
                ShapeSizeDialog ss = new ShapeSizeDialog();
                ss.Init(currentNode);
                ss.ShowDialog();
                if (currentNode.Dims.All(z => z > 0))
                    good = true;

            }

            if (!good) return;
            int size = 1;
            for (int i = 0; i < currentNode.Dims.Length; i++)
            {
                size *= currentNode.Dims[i];
            }
            var fl = new float[size];

            Random r = new Random(123);

            for (int i = 0; i < fl.Length; i++)
            {
                fl[i] = (float)r.NextDouble();
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

        private void setShapeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)(listView2.SelectedItems[0].Tag);

            ShapeSizeDialog ss = new ShapeSizeDialog();
            ss.Init(currentNode, false);
            ss.ShowDialog();
        }

        private void confToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            currentNode = (NodeInfo)((listView1.SelectedItems[0] as ListViewItem).Tag);
            if (OutputDatas.ContainsKey(currentNode.Name))
            {
                if (confToolStripMenuItem.Checked)
                {
                    if (!currentNode.Tags.Contains("conf"))
                    {
                        currentNode.Tags.Add("conf");
                    }
                }
                else
                {
                    currentNode.Tags.Remove("conf");
                }
                listView1.SelectedItems[0].SubItems[2].Text = string.Join(", ", currentNode.Tags);

            }
        }

        private void locationToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)((listView1.SelectedItems[0] as ListViewItem).Tag);
            if (OutputDatas.ContainsKey(currentNode.Name))
            {
                if (locationToolStripMenuItem.Checked)
                {
                    if (!currentNode.Tags.Contains("loc"))
                    {
                        currentNode.Tags.Add("loc");
                    }
                }
                else
                {
                    currentNode.Tags.Remove("loc");
                }
                listView1.SelectedItems[0].SubItems[2].Text = string.Join(", ", currentNode.Tags);
            }
        }



        private void loadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Dendrite inference environment (*.den)|*.den";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            LoadEnvironmentAsync(ofd.FileName);            
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Dendrite inference environment (*.den)|*.den";
            //zip arch
            if (sfd.ShowDialog() != DialogResult.OK) return;

            SaveModelAsync(sfd.FileName);
        }

        async void SaveModelAsync(string path)
        {
            await Task.Run(() =>
              {
                  using (var fileStream = new FileStream(path, FileMode.Create))
                  {
                      using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                      {
                          if (env.Pipeline.Nodes.Any(z => z is NetNode))
                          {
                              var netName = new FileInfo(net.NetPath).Name;
                              if (net.NetPath.EndsWith(".den"))
                              {
                                  netName = net.GetModelName();
                              }
                              var demoFile = archive.CreateEntry(netName);

                              using (var entryStream = demoFile.Open())
                              //using (var streamWriter = new StreamWriter(entryStream))
                              {
                                  if (net.NetPath.EndsWith(".den"))
                                  {
                                      var bytes = net.GetModelBytes();
                                      entryStream.Write(bytes, 0, bytes.Length);
                                  }
                                  else
                                  {
                                      using (var stream1 = File.OpenRead(net.NetPath))
                                      {
                                          stream1.CopyTo(entryStream);
                                      }
                                  }
                              }
                          }
                          var configFile = archive.CreateEntry("config.xml");

                          var configXml = env.GetConfigXml();
                          using (var entryStream = configFile.Open())
                          using (var streamWriter = new StreamWriter(entryStream))
                          {
                              streamWriter.WriteLine(configXml);
                          }

                          var uiConfigFile = archive.CreateEntry("ui.xml");

                          var uiConfigXml = pipelineUI.GetConfigXml();
                          using (var entryStream = uiConfigFile.Open())
                          using (var streamWriter = new StreamWriter(entryStream))
                          {
                              streamWriter.WriteLine(uiConfigXml);
                          }
                      }
                  }
              });
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            ctx.Update();
            pictureBox3.Invalidate();
        }

        private void boxesDecoderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void showImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Save("temp.jpg");
            Process.Start(new ProcessStartInfo("temp.jpg") { UseShellExecute = true });
        }

        private void postprocessorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //contextMenuStrip4.Show(Cursor.Position);
        }

        private void preprocessorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //contextMenuStrip2.Show(Cursor.Position);
        }
        DiskFilesystem DiskFilesystem = new DiskFilesystem();
        private void netToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "ONNX (*.onnx)|*.onnx";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            net.Init(DiskFilesystem, ofd.FileName);

            var nn = InferenceEnvironment.GenerateNodeFromNet(env.Net);
            env.Pipeline.Nodes.Add(nn);
            pipelineUI.AddItem(nn);
        }

        private void deleteToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (selected != null)
            {
                if (Extensions.ShowQuestion("Are you sure to delete selected nodes?", Text) == DialogResult.Yes)
                {
                    var n = (selected as NodeUI);
                    env.Pipeline.Nodes.Remove(n.Node);
                    n.Node.Detach();
                    pipelineUI.Elements.Remove(n);
                }
            }
        }

        private void loadImgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selected != null)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() != DialogResult.OK) return;
                var mat = Cv2.ImRead(ofd.FileName);
                if ((selected.Node is ImageSourceNode isn))
                {
                    isn.SourceMat = mat;
                }
            }
        }

        private void imageSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var nn = new ImageSourceNode();
            env.Pipeline.Nodes.Add(nn);
            pipelineUI.AddItem(nn);
        }

        private void showImgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selected != null)
            {
                var targets = new[] { selected.Node, selected.Node.Tag };
                foreach (var item in targets.OfType<IImageContainer>())
                {
                    if (item.Image != null)
                    {
                        Mat ret = item.Image;
                        ret.ConvertTo(ret, MatType.CV_8UC3);
                        if (item.Image.Channels() == 1)
                        {
                            ret = new Mat();
                            item.Image.ConvertTo(ret, MatType.CV_8UC3);
                        }
                        pictureBox1.Image = ret.ToBitmap();
                    }
                }
            }
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void priorBoxesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void concatToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem21_Click(object sender, EventArgs e)
        {
            var node = InferenceEnvironment.GenerateNodeFromProcessor(new YoloDecodePreprocessor());
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem22_Click(object sender, EventArgs e)
        {
            var r = new BoxesDecodePostProcessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem30_Click(object sender, EventArgs e)
        {
            var r = new BGR2RGBPreprocessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);

            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void oneHotVectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = new OneHotVectorPostProcessor() { };
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);

        }

        private void cRAFTDepthMapDecoderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = new CraftMapDecoderPostProcessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void blendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = new BlendingPostProcessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void imgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = new ImgProcessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var r = new ResizePreprocessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);

            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            var r = new NCHWPreprocessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);

            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            var r = new MeanStdPreprocessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);

            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            List<Node> nodes = new List<Node>();
            nodes.Add(new ImageSourceNode());
            nodes.Add(InferenceEnvironment.GenerateNodeFromProcessor(new ResizePreprocessor()));
            nodes.Add(InferenceEnvironment.GenerateNodeFromProcessor(new NormalizePreprocessor()));
            nodes.Add(InferenceEnvironment.GenerateNodeFromProcessor(new MeanStdPreprocessor()));
            nodes.Add(InferenceEnvironment.GenerateNodeFromProcessor(new NCHWPreprocessor()));

            int xx = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                Node? node = nodes[i];
                env.Pipeline.Nodes.Add(node);
                var nn = pipelineUI.AddItem(node);
                nn.Position = new PointF(xx, 0);
                xx += 200;
                if (i > 0)
                {
                    PinLink link = new PinLink() { Input = nodes[i - 1].Outputs[0], Output = nodes[i].Inputs[0] };
                    nodes[i - 1].Outputs[0].OutputLinks.Add(link);
                    nodes[i].Inputs[0].InputLinks.Add(link);
                }
            }
        }

        private void putTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = new PutTextPostProcessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void Processing_DragDrop(object sender, DragEventArgs e)
        {
            var ar = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (ar == null || ar.Length < 1) return;


            net.Init(DiskFilesystem, ar[0]);
            var nn = InferenceEnvironment.GenerateNodeFromNet(env.Net);
            env.Pipeline.Nodes.Add(nn);
            pipelineUI.AddItem(nn);
        }

        private void Processing_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            var r = new GrayscalePreprocessor();

            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);

            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            var r = new NormalizePreprocessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);

            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            var r = new ImgConcatPostProcessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            var r = new PriorBoxesProcessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            var r = new BGR2RGBPreprocessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            var r = new ZeroImagePreprocessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem26_Click(object sender, EventArgs e)
        {
            var r = new InstanceSegmentationDecodePreprocessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem27_Click(object sender, EventArgs e)
        {
            var r = new DrawInstanceSegmentationPostProcessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem23_Click(object sender, EventArgs e)
        {
            var r = new DrawBoxesPostProcessor() { };
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem28_Click(object sender, EventArgs e)
        {
            var r = new NmsPostProcessors();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);            
        }

        private void toolStripMenuItem24_Click(object sender, EventArgs e)
        {
            var r = new KeypointsDecodePreprocessor();

            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);

            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);
        }

        private void toolStripMenuItem25_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem29_Click(object sender, EventArgs e)
        {
            var r = new DepthmapDecodePreprocessor();
            var node = InferenceEnvironment.GenerateNodeFromProcessor(r);
            env.Pipeline.Nodes.Add(node);
            pipelineUI.AddItem(node);            
        }
    }
}