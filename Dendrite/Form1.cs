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
        public void DrawRoundedRectangle(Graphics g,
                                 RectangleF r, int d, Pen pen)
        {
            g.DrawPath(pen, GetRoundedRectangle(r, d));
        }
        public GraphicsPath GetRoundedRectangle(RectangleF r, int d)
        {
            if (d == 0) { d = 1; }
            GraphicsPath gp = new GraphicsPath();

            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.X + r.Width - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d,
                                                             0, 90);
            gp.AddArc(r.X, r.Y + r.Height - d, d, d, 90, 90);
            gp.AddLine(r.X, r.Y + r.Height - d, r.X, r.Y + d / 2);

            return gp;
        }

        public void FillRoundedRectangle(Graphics g,
                                RectangleF r, int d, Brush myBrush)
        {
            g.FillPath(myBrush, GetRoundedRectangle(r, d));
        }
        GraphNode hovered = null;
        Font f = new Font("Arial", 18);
        Font f2 = new Font("Arial", 14);
        Brush textBrush = Brushes.Black;
        private void drawEdges(IDrawingContext ctx)
        {
            if (Model.Edges != null && CurrentLayout.EdgesDrawAllowed)
                foreach (var item in Model.Edges)
                {
                    item.Draw(ctx);
                }
            else
                foreach (var item in Model.Nodes)
                {

                    var dtag = item.DrawTag as GraphNodeDrawInfo;
                    if (dtag == null) continue;
                    foreach (var citem in item.Childs)
                    {
                        var dtag2 = citem.DrawTag as GraphNodeDrawInfo;
                        if (dtag2 == null) continue;
                        var size = 6 * ctx.Zoom;
                        AdjustableArrowCap bigArrow = new AdjustableArrowCap(size, size, true);
                        Pen pen1 = new Pen(Color.Black);
                        pen1.CustomEndCap = bigArrow;

                        ctx.Graphics.DrawLine(pen1,
                            ctx.Transform(dtag.Rect.Location.X + dtag.Rect.Size.Width / 2, dtag.Rect.Location.Y + dtag.Rect.Height / 2),
                            ctx.Transform(dtag2.Rect.Location.X + dtag2.Rect.Size.Width / 2, dtag2.Rect.Location.Y + dtag2.Rect.Height / 2)
                            );
                    }
                }
        }

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
                    drawEdges(ctx);
                    drawNodes(ctx);
                    drawLabels(ctx);
                }

            }
            ctx.Box.Invoke((Action)(() =>
                {
                    ctx.Swap();
                    //ctx.Box.Refresh();
                }));
        }

        private void drawNodes(IDrawingContext ctx)
        {

            foreach (var item in Model.Nodes)
            {

                var dtag = item.DrawTag as GraphNodeDrawInfo;
                if (dtag == null) continue;
                var rr = ctx.Transform(dtag.Rect);
                var rr2 = ctx.Transform(new RectangleF(dtag.Rect.Left, dtag.Rect.Top, dtag.Rect.Width, dtag.Rect.Height));

                textBrush = Brushes.Black;

                int cornerRadius = (int)(15 * ctx.Zoom);
                Brush brush = Brushes.LightGray;
                textBrush = Brushes.White;
                switch (item.LayerType)
                {
                    case LayerType.Gemm:
                    case LayerType.Conv:
                    case LayerType.Lstm:
                        brush = StaticColors.ConvBrush;
                        break;
                    case LayerType.Dropout:
                        brush = StaticColors.DropoutBrush;
                        break;
                    case LayerType.Gather:
                    case LayerType.Squeeze:
                    case LayerType.Batch:
                        brush = StaticColors.BatchNormBrush;
                        break;
                    case LayerType.Log:
                    case LayerType.Softmax:
                    case LayerType.Relu:
                        brush = StaticColors.ReluBrush;
                        break;
                    case LayerType.PrimitiveMath:
                    case LayerType.MathOperation:
                        brush = StaticColors.MathBrush;
                        break;
                    case LayerType.Transpose:
                    case LayerType.Pool:
                        brush = StaticColors.PoolBrush;
                        break;
                    case LayerType.Input:
                    case LayerType.Output:
                        brush = StaticColors.EndpointBrush;
                        textBrush = Brushes.Black;
                        break;
                    case LayerType.Pad:
                    case LayerType.Concat:
                        brush = StaticColors.ConcatBrush;
                        break;
                }
                if (item == hovered)
                {
                    textBrush = Brushes.Black;
                    if (CurrentLayout.DrawHeadersAllowed && item.DrawHeader)
                    {
                        RectangleF headerRect = new RectangleF(rr2.Left, rr2.Top, rr2.Width, item.HeaderHeight * ctx.Zoom);
                        ctx.Graphics.FillPath(Brushes.White, Helpers.RoundedRect(rr2, cornerRadius));
                        ctx.Graphics.FillPath(Brushes.White, Helpers.HalfRoundedRect(headerRect, cornerRadius));
                        ctx.Graphics.DrawPath(Pens.Black, Helpers.HalfRoundedRect(headerRect, cornerRadius));

                    }
                    else
                    {
                        FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.Zoom), Brushes.LightYellow);

                    }
                }
                else if (CurrentLayout.FlashHoveredRelatives && item.Parents.Contains(hovered))
                {
                    FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.Zoom), Brushes.LightPink);
                }
                else if (CurrentLayout.FlashHoveredRelatives && item.Childs.Contains(hovered))
                {
                    FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.Zoom), Brushes.LightBlue);
                }
                else
                if (item == selected)
                {
                    textBrush = Brushes.Black;

                    if (CurrentLayout.DrawHeadersAllowed && item.DrawHeader)
                    {
                        RectangleF headerRect = new RectangleF(rr2.Left, rr2.Top, rr2.Width, item.HeaderHeight * ctx.Zoom);
                        ctx.Graphics.FillPath(Brushes.White, Helpers.RoundedRect(rr2, cornerRadius));
                        ctx.Graphics.FillPath(Brushes.LightGreen, Helpers.HalfRoundedRect(headerRect, cornerRadius));
                        ctx.Graphics.DrawPath(Pens.Black, Helpers.HalfRoundedRect(headerRect, cornerRadius));
                    }
                    else
                    {
                        FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.Zoom), Brushes.LightGreen);
                    }
                }
                else
                {
                    if (CurrentLayout.DrawHeadersAllowed && item.DrawHeader)
                    {
                        RectangleF headerRect = new RectangleF(rr2.Left, rr2.Top, rr2.Width, item.HeaderHeight * ctx.Zoom);
                        ctx.Graphics.FillPath(Brushes.White, Helpers.RoundedRect(rr2, cornerRadius));

                        ctx.Graphics.FillPath(brush, Helpers.HalfRoundedRect(headerRect, cornerRadius));
                        ctx.Graphics.DrawPath(Pens.Black, Helpers.HalfRoundedRect(headerRect, cornerRadius));

                    }
                    else
                    {
                        ctx.Graphics.FillPath(brush, Helpers.RoundedRect(rr2, cornerRadius));

                        //FillRoundedRectangle(ctx.Graphics, rr2, cornerRadius, brush);
                    }
                }
                ctx.Graphics.DrawPath(Pens.Black, Helpers.RoundedRect(rr2, cornerRadius));

                //DrawRoundedRectangle(ctx.Graphics, rr, (int)(40 * ctx.zoom), Pens.Black);


                ctx.Graphics.ResetTransform();
                var sh = ctx.Transform(dtag.Rect.Left, dtag.Rect.Top + 10);
                ctx.Graphics.TranslateTransform(sh.X, sh.Y);
                ctx.Graphics.ScaleTransform(ctx.Zoom, ctx.Zoom);
                //ctx.Graphics.DrawString($"{item.Name}: ({item.OpType})", f, Brushes.Black, 0, 0);
                if (ShowFullNames || item.LayerType == LayerType.Input || item.LayerType == LayerType.Output)
                {
                    var ms = ctx.Graphics.MeasureString($"{item.Name}:{item.OpType}", f);
                    ctx.Graphics.DrawString($"{item.Name}:{item.OpType}", f, textBrush, +dtag.Rect.Width / 2 - ms.Width / 2, 0);
                }
                else
                {
                    var ms = ctx.Graphics.MeasureString($"{item.OpType}", f);
                    ctx.Graphics.DrawString($"{item.OpType}", f, textBrush, +dtag.Rect.Width / 2 - ms.Width / 2, 0);
                }
                if (CurrentLayout.DrawHeadersAllowed && item.LayerType == LayerType.Conv)
                {
                    var fb = new Font(f.FontFamily, f.Size, FontStyle.Bold);
                    if (item.Data.Count > 0)
                    {

                        ctx.Graphics.DrawString("W:", fb, Brushes.Black, 5, 35);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[0].Dims)})", f, Brushes.Black, 45, 35);
                    }
                    if (item.Data.Count > 1)
                    {
                        ctx.Graphics.DrawString("B:", fb, Brushes.Black, 5, 65);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[1].Dims)})", f, Brushes.Black, 45, 65);
                    }
                }
                if (CurrentLayout.DrawHeadersAllowed && item.LayerType == LayerType.Gather)
                {
                    var fb = new Font(f.FontFamily, f.Size, FontStyle.Bold);
                    if (item.Attributes.Count > 0)
                    {
                        ctx.Graphics.DrawString("Indicies = ", f, Brushes.Black, 5, 35);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Attributes[0].IntData)})", f, Brushes.Black, 120, 35);
                    }

                }
                if (CurrentLayout.DrawHeadersAllowed && item.LayerType == LayerType.Gemm)
                {
                    var fb = new Font(f.FontFamily, f.Size, FontStyle.Bold);
                    if (item.Data.Count > 0)
                    {

                        ctx.Graphics.DrawString("B:", fb, Brushes.Black, 5, 35);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[0].Dims)})", f, Brushes.Black, 45, 35);
                    }
                    if (item.Data.Count > 1)
                    {
                        ctx.Graphics.DrawString("C:", fb, Brushes.Black, 5, 65);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[1].Dims)})", f, Brushes.Black, 45, 65);
                    }
                }
                if (CurrentLayout.DrawHeadersAllowed && item.LayerType == LayerType.Lstm)
                {
                    var fb = new Font(f.FontFamily, f.Size, FontStyle.Bold);
                    if (item.Data.Count > 0)
                    {

                        ctx.Graphics.DrawString("W:", fb, Brushes.Black, 5, 35);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[0].Dims)})", f, Brushes.Black, 45, 35);
                    }
                    if (item.Data.Count > 1)
                    {
                        ctx.Graphics.DrawString("R:", fb, Brushes.Black, 5, 65);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[1].Dims)})", f, Brushes.Black, 45, 65);
                    }
                    if (item.Data.Count > 2)
                    {
                        ctx.Graphics.DrawString("B:", fb, Brushes.Black, 5, 95);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[2].Dims)})", f, Brushes.Black, 45, 95);
                    }
                }
                if (CurrentLayout.DrawHeadersAllowed && item.LayerType == LayerType.Batch)
                {
                    var fb = new Font(f.FontFamily, f.Size, FontStyle.Bold);
                    if (item.Data.Count > 0)
                    {

                        ctx.Graphics.DrawString("scale:", fb, Brushes.Black, 5, 35);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[0].Dims)})", f, Brushes.Black, 85, 35);
                    }
                    if (item.Data.Count > 1)
                    {
                        ctx.Graphics.DrawString("B:", fb, Brushes.Black, 5, 65);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[1].Dims)})", f, Brushes.Black, 85, 65);
                    }
                    if (item.Data.Count > 2)
                    {
                        ctx.Graphics.DrawString("mean:", fb, Brushes.Black, 5, 95);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[2].Dims)})", f, Brushes.Black, 85, 95);
                    }
                    if (item.Data.Count > 3)
                    {
                        ctx.Graphics.DrawString("var:", fb, Brushes.Black, 5, 125);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[3].Dims)})", f, Brushes.Black, 85, 125);
                    }
                }
                if (CurrentLayout.DrawHeadersAllowed && item.LayerType == LayerType.MathOperation)
                {
                    var fb = new Font(f.FontFamily, f.Size, FontStyle.Bold);
                    if (item.Data.Count > 0)
                    {
                        ctx.Graphics.DrawString("B:", fb, Brushes.Black, 5, 35);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[0].Dims)})", f, Brushes.Black, 45, 35);
                    }

                }
                var ms2 = ctx.Graphics.MeasureString(item.OpType, f);
                //ctx.Graphics.DrawString(item.OpType, f, textBrush, +dtag.Rect.Width / 2 - ms2.Width / 2, 30);
                if (CurrentLayout.FlashHoveredRelatives)
                {
                    if (item.Parents.Contains(hovered))
                    {
                        ctx.Graphics.DrawString("child", f, textBrush, +dtag.Rect.Width / 2 - ms2.Width / 2, 60);
                    }
                    if (item.Childs.Contains(hovered))
                    {
                        ctx.Graphics.DrawString("parent", f, textBrush, +dtag.Rect.Width / 2 - ms2.Width / 2, 60);
                    }
                }


                ctx.Graphics.ResetTransform();
            }
        }
        private void drawLabels(IDrawingContext ctx)
        {
            foreach (var item in Model.Nodes)
            {

                var dtag = item.DrawTag as GraphNodeDrawInfo;
                if (dtag == null) continue;


                if (item.Shape != null)
                {
                    ctx.Graphics.ResetTransform();
                    var sh = ctx.Transform(dtag.Rect.Left, dtag.Rect.Bottom + 10);
                    ctx.Graphics.TranslateTransform(sh.X, sh.Y);
                    ctx.Graphics.ScaleTransform(ctx.Zoom, ctx.Zoom);

                    var s1 = string.Join("x", item.Shape);


                    ctx.Graphics.DrawString(s1, f2, textBrush, +dtag.Rect.Width / 2 + 10, 0);
                    ctx.Graphics.ResetTransform();
                }
            }
        }

        public bool ShowFullNames { get; set; } = false;
        Thread drawThread;
        public void StartDrawThread()
        {
            if (drawThread != null) return;
            drawThread = new Thread(() =>
            {
                while (true)
                {
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
        public bool LoadModel(string path)
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

                    var ms = ctx.Graphics.MeasureString(str, f);
                    return ms.Width;
                };
              
                CurrentLayout.Layout(Model);
               
                //Text = $"{WindowCaption}: {Path.GetFileName(path)}";
                if (ParentForm != null)
                {
                    ParentForm.Text = Path.GetFileName(path);
                }
                drawEnabled = true;
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

            foreach (var item in Model.Nodes)
            {
                var dtag = item.DrawTag as GraphNodeDrawInfo;
                if (dtag == null) continue;
                var rr = ctx.Transform(dtag.Rect);
                var rr1 = GetRoundedRectangle(rr, (int)(40 * ctx.zoom));
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
            bool exit = true;
            if (selected.Tag is NodeProto) exit = false;
            if (selected.Tag is ValueInfoProto) exit = false;
            if (exit) return;

            Edit ee = new Edit();
            if (Model is OnnxGraphModel gm)
            {
                if (selected.Tag is NodeProto np)
                {
                    ee.Init(gm.ProtoModel, np);
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

        public static int ExportImageMaxDim=4000;
        private void exportAsImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Jpeg files (*.jpg)|*.jpg|All files (*.*)|*.*";
            if (sfd.ShowDialog() != DialogResult.OK) return;

            BitmapDrawingContext ectx = new BitmapDrawingContext();
            var ar = Model.Nodes.Where(z => z.DrawTag != null).Select(z => z.DrawTag as GraphNodeDrawInfo).ToArray();
            var minx = ar.Min(z => z.X);
            var miny = ar.Min(z => z.Y);
            var maxx = ar.Max(z => z.X);
            var maxy = ar.Max(z => z.Y);
            var ww = (int)Math.Ceiling(maxx - minx);
            var hh = (int)Math.Ceiling(maxy - miny);
            var maxDim = Math.Max(ww, hh);
            Bitmap bmp = null;
            if (maxDim > ExportImageMaxDim)
            {
                ectx.Zoom = (float)ExportImageMaxDim / maxDim;
                bmp = new Bitmap((int)(ww * ectx.Zoom), (int)(hh * ectx.Zoom));
            }
            else
            {
                bmp = new Bitmap(ww, hh);
            }

            ectx.Bmp = bmp;
            //fit all
            List<PointF> pp = new List<PointF>();
            foreach (var item in Model.Nodes)
            {
                var dtag = item.DrawTag as GraphNodeDrawInfo;
                pp.Add(dtag.Rect.Location);
                pp.Add(new PointF(dtag.Rect.Right, dtag.Rect.Bottom));
            }

            ectx.FitToPoints(pp.ToArray(), 5);
            ectx.Graphics = Graphics.FromImage(bmp);
            ectx.Graphics.Clear(Color.White);
            ectx.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            drawEdges(ectx);
            drawLabels(ectx);
            drawNodes(ectx);

            bmp.Save(sfd.FileName);
            Program.MainForm.SetStatusMessage("Successfully saved: " + sfd.FileName);

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

        private void showHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLayout.VerticalLayout = false;
            if (File.Exists(_lastPath))
            {
                LoadModel(_lastPath);
                fitAll();
            }
        }
    }
}
