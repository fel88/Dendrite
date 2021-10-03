using Onnx;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Xml.Linq;
using System.IO.Compression;
using Dendrite.Layouts;

namespace Dendrite
{
    public partial class Form1 : UserControl
    {
        public Form1()
        {
            InitializeComponent();

            if (File.Exists("settings.xml"))
            {
                var doc = XDocument.Load("settings.xml");
                foreach (var item in doc.Descendants("recents"))
                {
                    var fi = new FileInfo(item.Element("path").Value);
                    var tt = new ToolStripMenuItem(fi.FullName) { Tag = fi };
                    tt.Click += (s, e) =>
                    {
                        LoadModel(((s as ToolStripMenuItem).Tag as FileInfo).FullName);
                    };
                    recentToolStripMenuItem.DropDownItems.Add(tt);
                }
            }

            ctx.Init(pictureBox1);
            pictureBox1.SetDoubleBuffered(true);
            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            ctx.Redraw = Redraw;
            //ParseTensorFromString("");

            Providers.Add(new OnnxModelProvider());
            Providers.Add(new TorchScriptModelProvider());

            group1.Width = panel1.Width;
            group1.Height = 100;
            panel1.Controls.Add(group1);
            group1.SetCaption("Node Properties");

            group2.Width = panel1.Width;

            group2.Top = group1.Bottom;
            panel1.Controls.Add(group2);
            group2.SetCaption("Attributes");

            group3.Width = panel1.Width;
            group3.Top = group2.Bottom;
            panel1.Controls.Add(group3);
            group3.SetCaption("Inputs");


            group4.Width = panel1.Width;
            group4.Top = group3.Bottom;
            panel1.Controls.Add(group4);
            group4.SetCaption("Outputs");

            CurrentLayout = Activator.CreateInstance(DefaultLayout) as GraphLayout;

            pictureBox1.Focus();
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            HideInfoTab();

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                LoadModel(args[1]);
            }
            Load += Form1_Load;
        }

        public static Type DefaultLayout = typeof(TableGraphLayout);
        private void Form1_Load(object sender, EventArgs e)
        {
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);
        }

        MessageFilter mf = null;
        private void ParentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<root>");
            sb.AppendLine("<recents>");
            foreach (var item in loadedModels)
            {

            }
            sb.AppendLine("<recents>");
            sb.AppendLine("</root>");

        }

        ExpandGroupControl group1 = new ExpandGroupControl();
        ExpandGroupControl group2 = new ExpandGroupControl();
        ExpandGroupControl group3 = new ExpandGroupControl();
        ExpandGroupControl group4 = new ExpandGroupControl();

        GraphNode selected = null;

        public Tuple<long[], float[]> ParseTensorFromString(string data)
        {


            long[] dims;
            float[] w;

            Stack<char> s = new Stack<char>();
            int max = 0;
            foreach (var item in data)
            {
                max = Math.Max(s.Count, max);
                if (item == '[')
                {
                    s.Push(item);
                }
                if (item == ']')
                {
                    if (s.Peek() != '[') throw new DataException();
                    s.Pop();
                }
            }
            if (s.Any()) throw new DataException();
            dims = new long[max];
            var temp = new long[max];
            w = data.Split(new char[] { ',', '[', ']', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Select(z => float.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            foreach (var item in data)
            {
                max = Math.Max(s.Count, max);
                if (item == '[')
                {
                    s.Push(item);
                }
                if (item == ']')
                {
                    dims[s.Count - 1] = temp[s.Count - 1] + 1;
                    temp[s.Count - 1] = 0; ;
                    if (s.Peek() != '[') throw new DataException();
                    s.Pop();
                }
                if (item == ',') { temp[s.Count - 1]++; }
            }
            return new Tuple<long[], float[]>(dims, w);
        }

        internal void StopDrawThread()
        {
            if (drawThread != null)
                drawThread.Abort();
        }

        public static void Rec(StringBuilder sb, long[] dims, int level, float[] array, long offset, int? lenLimit = null)
        {
            if (lenLimit != null && sb.Length > lenLimit) return;
            if (level == dims.Length - 1)
            {
                sb.Append("[");
                for (int i = 0; i < dims[level]; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(array[i + offset]);
                }
                sb.Append("]");
                return;
            }
            sb.Append("[");
            for (int i = 0; i < dims[level]; i++)
            {
                if (i > 0) sb.Append(",");
                Rec(sb, dims, level + 1, array, offset, lenLimit);
                offset += dims[level + 1];
            }
            sb.Append("]");
        }



        public static string GetFormattedArray(InputData data, int? lenLimit = null)
        {
            StringBuilder sb = new StringBuilder();

            Rec(sb, data.Dims, 0, data.Weights, 0, lenLimit);
            return sb.ToString();
        }

        public void UpdateInfo()
        {
            group1.Clear();

            group1.SetModel(Model);

            group1.AddItem("type", $"{selected.OpType}:{selected.LayerType}", Model);
            group1.AddItem("name", selected.Name, Model, selected);
            group2.Clear();
            group2.Top = group1.Bottom;
            group2.SetModel(Model);


            foreach (var item in selected.Attributes)
            {
                string vall = "";
                switch (item.Type)
                {
                    case AttributeInfoDataType.Int:
                        vall = item.IntData + "";
                        break;
                    case AttributeInfoDataType.Ints:
                        if (item.Ints.Count > 20)
                        {
                            vall = item.Ints.Take(20).Aggregate("", (x, y) => x + y + ", ");
                        }
                        else
                        {
                            vall = item.Ints.Aggregate("", (x, y) => x + y + ", ");
                        }
                        break;
                    case AttributeInfoDataType.Float32:
                        vall = item.FloatData + "";
                        break;
                    case AttributeInfoDataType.String:
                        vall = item.StringData;
                        break;
                    case AttributeInfoDataType.Floats:
                        vall = item.Floats.Aggregate("", (x, y) => x + y + ", ");
                        break;
                }
                group2.AddItem(item.Name, vall, Model, item);
            }
            group3.Clear();
            group3.Top = group2.Bottom;
            if (selected.Parent != null)
            {
                group3.AddItem("name", "input:" + selected.Input, Model);

            }
            foreach (var item in selected.Data)
            {
                var res = group3.AddItem("", "name:" + item.Name, Model);
                if (item.Dims != null)
                {
                    res.AddSubItem("dims", item.Dims.Aggregate("", (x, y) => x + y + ","));
                }
                if (item.Weights != null)
                {
                    if (item.Weights.Length > 10)
                    {
                        res.AddSubItem("data", item.Weights.Take(10).Aggregate("", (x, y) => x + y + ","), () =>
                        {
                            return GetFormattedArray(item);
                        });
                    }
                    else
                    {
                        res.AddSubItem("data", item.Weights.Aggregate("", (x, y) => x + y + ","));
                    }
                    res.PlusVisible = true;
                    //item.Dims.Aggregate("", (x, y) => x + y + ", ")
                }
            }

            group4.Clear();
            group4.Top = group3.Bottom;
            if (selected.Tag != null)
            {
                if (selected.Tag is NodeProto tag)
                {
                    if (tag.Output.Any())
                    {
                        group4.AddItem("Y", "name:" + tag.Output[0], Model);
                    }
                }
            }
        }


        private void ShowInfoTab()
        {
            tableLayoutPanel1.ColumnStyles[1].Width = 250;
        }
        private void HideInfoTab()
        {
            tableLayoutPanel1.ColumnStyles[1].Width = 0;
        }
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (hovered == null) return;
            selected = hovered;
            ShowInfoTab();
            UpdateInfo();
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //pictureBox1.Focus();
        }

        GraphNode hovered = null;
        Font f = new Font("Arial", 18);
        Font f2 = new Font("Arial", 14);
        Brush textBrush = Brushes.Black;

        bool drawEnabled = true;
        void Redraw()
        {
            if (ParentForm != null)
            {
                bool exit = false;
                ParentForm.Invoke((Action)(() =>
                {
                    if (Program.MainForm.ActiveMdiChild != ParentForm) exit = true;
                }));
                if (exit) return;

            }
            textBrush = Brushes.Black;
            lock (DrawingContext.lock1)
            {
                ctx.Update();

                ctx.Graphics.Clear(Color.White);
                ctx.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                ctx.Graphics.ResetTransform();

                ///axis
                //ctx.Graphics.DrawLine(Pens.Red, ctx.Transform(new PointF(0, 0)), ctx.Transform(new PointF(1000, 0)));
                //ctx.Graphics.DrawLine(Pens.Blue, ctx.Transform(new PointF(0, 0)), ctx.Transform(new PointF(0, 1000)));


                if (Model != null && drawEnabled)
                {
                    GraphDrawer.DrawEdges(ctx, Model, CurrentLayout);
                    GraphDrawer.DrawLabels(ctx, Model, f2, textBrush);
                    GraphDrawer.DrawNodes(ctx, Model, CurrentLayout, textBrush, f, selected, hovered, ShowFullNames);
                }

            }
            ctx.Box.Invoke((Action)(() =>
                {
                    ctx.Swap();
                    //ctx.Box.Refresh();
                }));
        }


        public bool ShowFullNames { get; set; } = false;
        Thread drawThread;
        AutoResetEvent reset = new AutoResetEvent(true);
        public void StartDrawThread()
        {
            if (drawThread != null) return;
            drawThread = new Thread(() =>
            {
                while (true)
                {
                    if (!drawEnabled)
                        reset.WaitOne();
                    Redraw();
                    Thread.Sleep(15);
                }
            });
            drawThread.IsBackground = true;
            drawThread.Start();
        }


        DrawingContext ctx = new DrawingContext();

        public List<ModelProvider> Providers = new List<ModelProvider>();


        public const string WindowCaption = "Dendrite";
        private string _lastPath;
        public GraphModel Model;

        public List<string> loadedModels = new List<string>();
        public bool LoadModel(string path, bool _fitAll = true)
        {
            if (ParentForm != null)
            {
                ParentForm.FormClosing += ParentForm_FormClosing;
            }
            _lastPath = path;
            var fr = Providers.FirstOrDefault(z => z.IsSuitableFile(path));
            if (fr == null)
            {
                MessageBox.Show("Unsupported file format.", WindowCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            Stopwatch sw = new Stopwatch();
            WaitDialog wd = new WaitDialog();
            timer1.Enabled = false;
            Action loadAct = () =>
            {
                sw.Start();
                var model = fr.LoadFromFile(path);
                Model = model;
                if (!loadedModels.Any(z => z.ToLower() == path.ToLower()))
                {
                    loadedModels.Add(path);
                }

                listView1.Items.Clear();

                foreach (var item in model.Nodes)
                {
                    //listView1.Items.Add(new ListViewItem(new string[] { item.Name, ss, item.Output[0] }) { Tag = nodes[i] });
                }

                //var cnt2 = res2.Graph.Output[0].Name;
                //nodes.InsertRange(0, res2.Graph.Input.Select(z => outs[z.Name]));

                updateNodesSizes();
                CurrentLayout.GetRenderTextWidth = (item) =>
                {
                    var str = string.Empty;
                    if (ShowFullNames || item.LayerType == LayerType.Input || item.LayerType == LayerType.Output)
                    {
                        str = $"{item.Name}:{item.OpType}";
                    }
                    else
                    {
                        str = $"{item.OpType}";
                    }


                    //var ms = ctx.Graphics.MeasureString(str, f);
                    var ms = TextRenderer.MeasureText(str, f);
                    return ms.Width;
                };
                CurrentLayout.Progress = (f) =>
                {
                    wd.SetProgress(f);
                };
                CurrentLayout.Layout(Model);

                //Text = $"{WindowCaption}: {Path.GetFileName(path)}";

                if (ParentForm != null)
                {
                    ParentForm.Invoke((Action)(() =>
                    {
                        ParentForm.Text = Path.GetFileName(path);
                    }));

                }
                drawEnabled = true;
                reset.Set();
                if (_fitAll)
                    fitAll();

                sw.Stop();
            };
            drawEnabled = false;

            wd.Init(loadAct);
            wd.ShowDialog();
            timer1.Enabled = true;
            if (wd.Exception != null)
            {
                Helpers.ShowError(wd.Exception.Message, Program.MainForm.Text);
            }
            Program.MainForm.SetStatusMessage($"Load time: {sw.ElapsedMilliseconds} ms");

            return true;
        }

        void updateNodesSizes()
        {
            foreach (var item in Model.Nodes)
            {
                GraphNodeDrawInfo dd = new GraphNodeDrawInfo() { X = 0, Y = 0, Width = 300, Height = 100 };
                item.DrawTag = dd;
                if (item.LayerType == LayerType.Conv || item.LayerType == LayerType.Lstm || item.LayerType == LayerType.Gather || item.LayerType == LayerType.Batch || (item.LayerType == LayerType.MathOperation && item.Parents.Count < 2) || item.LayerType == LayerType.Gemm)
                {
                    item.DrawHeader = true;
                }
            }
        }

        public GraphLayout CurrentLayout;


        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            ctx.sx = 0;
            ctx.sy = 0;
            ctx.zoom = 1;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var pos = pictureBox1.PointToClient(Cursor.Position);

            GraphNode hovered2 = null;

            if (Model == null) return;

            foreach (var item in Model.Nodes.Union(Model.Groups))
            {
                var dtag = item.DrawTag as GraphNodeDrawInfo;
                if (dtag == null) continue;
                var rr = ctx.Transform(dtag.Rect);
                var rr1 = GraphDrawer.GetRoundedRectangle(rr, (int)(40 * ctx.zoom));
                if (rr1.IsVisible(pos))
                {
                    hovered2 = item;
                    break;
                }
            }
            hovered = hovered2;
            //Redraw();
        }

        public void UpdateSearch()
        {
            var gg = Model.Nodes.Where(z => z.Name.ToLower().Contains(textBox1.Text.ToLower()));
            listView1.Items.Clear();
            foreach (var item in gg)
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.Name, item.OpType, "" }) { Tag = item });
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            UpdateSearch();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var lvi = listView1.SelectedItems[0] as ListViewItem;
            var gn = lvi.Tag as GraphNode;
            var rect = (gn.DrawTag as GraphNodeDrawInfo).Rect;

            ctx.zoom = 1;
            ctx.sx = -rect.X;
            ctx.sy = -rect.Y;
            selected = gn;
            UpdateInfo();

        }

        private void fromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_lastPath.EndsWith("onnx")) { MessageBox.Show("only onnx model supported", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            Processing p = new Processing();
            p.Init(_lastPath);
            p.ShowDialog();

        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var ar = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (ar == null) return;
            //if (Model == null)
            {
                LoadModel(ar[0]);
                //return;
            }
            /* switch (MessageBox.Show("Load another process?", WindowCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
             {
                 case DialogResult.Yes:
                     Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location, $"\"{ar[0]}\"");
                     break;
                 case DialogResult.No:
                     LoadModel(ar[0]);
                     break;
                 case DialogResult.Cancel:
                     break;
             }*/

        }

        private void fromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {

        }

        private void singleToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void multidocumentToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton5_Click_1(object sender, EventArgs e)
        {
            if (tableLayoutPanel1.ColumnStyles[1].Width == 0)
            {
                ShowInfoTab();
                toolStripButton5.Text = "hide";
            }
            else
            {
                toolStripButton5.Text = "show";

                HideInfoTab();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tableLayoutPanel1.RowStyles[0].Height = groupBox1.Height;
        }

        private void tableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLayout = new TableGraphLayout();
            CurrentLayout.Layout(Model);
            fitAll();

        }

        private void simpleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLayout = new SimpleGraphLayout();
            CurrentLayout.Layout(Model);
            fitAll();

        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (selected == null)
            {
                MessageBox.Show($"Select any node to append it to outputs.", WindowCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show($"Append {selected.Name} to outputs?", WindowCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            Model.Provider.AppendToOutput(Model, selected);
            MessageBox.Show($"Selected node was appended to outputs. Save model and reload to take effect.", WindowCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dagreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLayout = new DagreGraphLayout();
            WaitDialog wd = new WaitDialog();
            drawEnabled = false;
            wd.Init(() => { CurrentLayout.Layout(Model); drawEnabled = true; });
            wd.ShowDialog();
            fitAll();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "ONNX files (*.onnx)|*.onnx|All files (*.*)|*.*";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            LoadModel(ofd.FileName);
            fitAll();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Model == null) { MessageBox.Show("load model first", WindowCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = Model.Provider.SaveDialogFilter;
            if (sfd.ShowDialog() != DialogResult.OK) return;
            Model.Provider.SaveModel(Model, sfd.FileName);
        }

        public void ExportAsZip(string zipPath, GraphModel model)
        {
            int cntr = 0;
            if (File.Exists(zipPath)) File.Delete(zipPath);
            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.CreateNew))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<?xml version=\"1.0\"?>");
                    sb.AppendLine("<net>");

                    ZipArchiveEntry wg = archive.CreateEntry("weights/");
                    foreach (var item in model.Nodes)
                    {
                        sb.AppendLine($"<node id=\"{item.Id}\" name=\"{item.Name}\" opType=\"{item.LayerType.ToString()}\">");
                        sb.AppendLine("<attributes>");
                        foreach (var attr in item.Attributes)
                        {
                            sb.AppendLine($"<attr name=\"{attr.Name}\" value=\"{(attr.Tag as AttributeProto).AsString()}\"/>");
                        }
                        sb.AppendLine("</attributes>");
                        sb.AppendLine("<childs>");
                        foreach (var attr in item.Childs)
                        {
                            sb.AppendLine($"<child id=\"{attr.Id}\"/>");
                        }
                        sb.AppendLine("</childs>");
                        sb.AppendLine("<parents>");
                        foreach (var attr in item.Parents)
                        {
                            sb.AppendLine($"<parent id=\"{attr.Id}\"/>");
                        }
                        sb.AppendLine("</parents>");
                        foreach (var w in item.Data)
                        {
                            if (w.Weights == null || w.Weights.Length == 0) continue;
                            sb.AppendLine($"<weights file=\"{cntr}.dat\" name=\"{w.Name}\" dims=\"{w.Dims.Aggregate("", (x, y) => x + y + ",").TrimEnd(',')}\"/>");


                            List<byte> bb = new List<byte>();
                            MemoryStream ms = new MemoryStream();

                            foreach (var bitem in w.Weights)
                            {
                                foreach (var b in BitConverter.GetBytes(bitem))
                                {
                                    ms.WriteByte(b);
                                }

                            }
                            ms.Seek(0, SeekOrigin.Begin);
                            ZipArchiveEntry w1 = archive.CreateEntry($"weights/{cntr++}.dat");

                            using (StreamWriter writer = new StreamWriter(w1.Open()))
                            {
                                ms.CopyTo(writer.BaseStream);
                            }
                        }
                        sb.AppendLine($"</node>");
                    }



                    sb.AppendLine("</net>");
                    var xpath = "info.xml";

                    ZipArchiveEntry ann = archive.CreateEntry(xpath);

                    using (StreamWriter writer = new StreamWriter(ann.Open()))
                    {
                        writer.Write(sb.ToString());
                    }


                }
            }

        }
        private void extractWeightsToZipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK) return;
            ExportAsZip(sfd.FileName, Model);
        }

        void edit()
        {
            if (selected == null) return;
            if (selected is GroupNode g)
            {
                if (Helpers.ShowQuestion($"Expand group {g.Prefix}?", ParentForm.Text) == DialogResult.Yes)
                {
                    CurrentLayout.RequestedGroups.RemoveAll(z => z.Prefix == g.Prefix);
                    LoadModel(_lastPath, false);
                    return;
                }
            }
            bool exit = true;
            if (selected.Tag is NodeProto) exit = false;
            if (selected.Tag is ValueInfoProto) exit = false;
            if (exit) return;

            Edit ee = new Edit();
            if (Model is OnnxGraphModel gm)
            {
                if (selected.Tag is NodeProto np)
                {
                    ee.Init(gm.ProtoModel, np, selected);
                    ee.ShowDialog(ParentForm);
                }
                if (selected.Tag is ValueInfoProto vip)
                {
                    //ee.Init(gm.ProtoModel, vip);
                    //ee.ShowDialog(ParentForm);
                }
            }
        }
        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            edit();
        }

        void fitAll()
        {
            List<PointF> pp = new List<PointF>();
            foreach (var item in Model.Nodes)
            {
                var dtag = item.DrawTag as GraphNodeDrawInfo;
                pp.Add(dtag.Rect.Location);
                pp.Add(new PointF(dtag.Rect.Right, dtag.Rect.Bottom));
            }

            ctx.FitToPoints(pp.ToArray(), 5);
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            fitAll();
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ctx.StopDrag();
            edit();
        }

        private void fullNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowFullNames = fullNamesToolStripMenuItem.Checked;
        }

        public static int ExportImageMaxDim = 4000;
        private void exportAsImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Jpeg files (*.jpg)|*.jpg|All files (*.*)|*.*";
            if (sfd.ShowDialog() != DialogResult.OK) return;

            var bmp = GraphDrawer.ExportImage(Model, textBrush, f, f2, ShowFullNames, ExportImageMaxDim);
            bmp.Save(sfd.FileName);
            Program.MainForm.SetStatusMessage("Successfully saved: " + sfd.FileName);

        }

        private void horizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLayout.VerticalLayout = false;

            if (File.Exists(_lastPath))
            {
                LoadModel(_lastPath);
                fitAll();
            }
        }

        private void showVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLayout.VerticalLayout = true;

            if (File.Exists(_lastPath))
            {
                LoadModel(_lastPath);
                fitAll();
            }
        }
                
        private void collapseGroupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> names = new List<string>  { "enc1.", "enc2.", "enc3.", "enc4.", "enc5.", "enc6.", "dec1.", "dec2.", "dec3.",
                "bert/embeddings"
            };
            for (int i = 0; i <= 11; i++)
            {
                //names.Add("bert/encoder/layer_");
                //names.Add("bert/encoder/layer_" + i );
                // names.Add("bert/encoder/layer_" + i + "/attention");
                // names.Add("bert/encoder/layer_" + i + "/output");
                // names.Add("bert/encoder/layer_" + i + "/intermediate");
            }
            for (int i = 0; i <= 11; i++)
            {
                
                //names.Add("bert/encoder/layer_" + i );
                 names.Add("bert/encoder/layer_" + i + "/attention");
                 names.Add("bert/encoder/layer_" + i + "/output");
                 names.Add("bert/encoder/layer_" + i + "/intermediate");
            }
            //string[] names = new[] { "enc1.","enc2."};
            var ww = Model.Nodes.Where(z => names.Any(u => z.Name.StartsWith(u))).ToArray();
            if (ww.Any())
            {
                CurrentLayout.RequestedGroups.Clear();
                foreach (var item in ww)
                {
                    var fr = names.First(z => item.Name.StartsWith(z));
                    if (!CurrentLayout.RequestedGroups.Any(z => z.Prefix == fr))
                    {
                        CurrentLayout.RequestedGroups.Add(new GroupNode() { Prefix = fr });
                    }
                }

                LoadModel(_lastPath, false);
            }
        }

        private void showGroupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLayout.RequestedGroups.Clear();
            LoadModel(_lastPath, false);
        }

        private void clusterToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
