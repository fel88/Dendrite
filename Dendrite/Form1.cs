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

namespace Dendrite
{
    public partial class Form1 : UserControl
    {
        public Form1()
        {
            InitializeComponent();
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

            CurrentLayout = TableLayoutGraph;


            pictureBox1.Focus();
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            HideInfoTab();

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                LoadModel(args[1]);
            }
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
            drawThread.Abort();
        }

        void Rec(StringBuilder sb, long[] dims, int level, float[] array, long offset)
        {
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
                Rec(sb, dims, level + 1, array, offset);
                offset += dims[level + 1];
            }
            sb.Append("]");
        }



        public string GetFormattedArray(InputData data)
        {
            StringBuilder sb = new StringBuilder();

            Rec(sb, data.Dims, 0, data.Weights, 0);
            return sb.ToString();
        }

        public void UpdateInfo()
        {
            group1.Clear();
            group1.AddItem("type", $"{selected.OpType}:{selected.LayerType}");
            group1.AddItem("name", selected.Name);
            group2.Clear();
            group2.Top = group1.Bottom;
            foreach (var item in selected.Attributes)
            {
                string vall = "";
                switch (item.Type)
                {
                    case AttributeInfoDataType.Int:
                        vall = item.IntData + "";
                        break;
                    case AttributeInfoDataType.Ints:
                        vall = item.Ints.Aggregate("", (x, y) => x + y + ", ");
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
                group2.AddItem(item.Name, vall);
            }
            group3.Clear();
            group3.Top = group2.Bottom;
            if (selected.Parent != null)
            {
                group3.AddItem("name", "input:" + selected.Input);

            }
            foreach (var item in selected.Data)
            {
                var res = group3.AddItem("", "name:" + item.Name);
                res.AddSubItem("dims", item.Dims.Aggregate("", (x, y) => x + y + ","));
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
                var tag = selected.Tag as NodeProto;

                if (tag.Output.Any())
                {
                    group4.AddItem("Y", "name:" + tag.Output[0]);
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
            pictureBox1.Focus();
        }
        public void DrawRoundedRectangle(Graphics g,
                                 Rectangle r, int d, Pen pen)
        {
            g.DrawPath(pen, GetRoundedRectangle(r, d));
        }
        public GraphicsPath GetRoundedRectangle(Rectangle r, int d)
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
                                Rectangle r, int d, Brush myBrush)
        {
            g.FillPath(myBrush, GetRoundedRectangle(r, d));
        }
        GraphNode hovered = null;
        Font f = new Font("Arial", 18);
        Font f2 = new Font("Arial", 14);

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
            lock (DrawingContext.lock1)
            {
                ctx.Update();

                ctx.Graphics.Clear(Color.White);
                ctx.Graphics.SmoothingMode = SmoothingMode.AntiAlias;


                ctx.Graphics.ResetTransform();

                ///axis
                //ctx.Graphics.DrawLine(Pens.Red, ctx.Transform(new PointF(0, 0)), ctx.Transform(new PointF(1000, 0)));
                //ctx.Graphics.DrawLine(Pens.Blue, ctx.Transform(new PointF(0, 0)), ctx.Transform(new PointF(0, 1000)));
                Brush textBrush = Brushes.Black;

                if (Model != null)
                {
                    foreach (var item in Model.Nodes)
                    {

                        var dtag = item.DrawTag as GraphNodeDrawInfo;
                        if (dtag == null) continue;
                        foreach (var citem in item.Childs)
                        {
                            var dtag2 = citem.DrawTag as GraphNodeDrawInfo;
                            if (dtag2 == null) continue;

                            ctx.Graphics.DrawLine(Pens.Black,
                                ctx.Transform(dtag.Rect.Location.X + dtag.Rect.Size.Width / 2, dtag.Rect.Location.Y + dtag.Rect.Height / 2),
                                ctx.Transform(dtag2.Rect.Location.X + dtag2.Rect.Size.Width / 2, dtag2.Rect.Location.Y + dtag2.Rect.Height / 2)
                                );

                        }

                        if (item.Shape != null)
                        {
                            ctx.Graphics.ResetTransform();
                            var sh = ctx.Transform(dtag.Rect.Left, dtag.Rect.Bottom + 10);
                            ctx.Graphics.TranslateTransform(sh.X, sh.Y);
                            ctx.Graphics.ScaleTransform(ctx.zoom, ctx.zoom);

                            var s1 = string.Join("x", item.Shape);


                            ctx.Graphics.DrawString(s1, f2, textBrush, +dtag.Rect.Width / 2 + 10, 0);
                            ctx.Graphics.ResetTransform();
                        }
                    }


                    foreach (var item in Model.Nodes)
                    {

                        var dtag = item.DrawTag as GraphNodeDrawInfo;
                        if (dtag == null) continue;
                        var rr = ctx.Transform(dtag.Rect);
                        var rr2 = ctx.Transform(new Rectangle(dtag.Rect.Left, dtag.Rect.Top, dtag.Rect.Width, dtag.Rect.Height));

                        textBrush = Brushes.Black;


                        if (item == hovered)
                        {
                            FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.zoom), Brushes.LightYellow);
                        }
                        else if (item.Parents.Contains(hovered))
                        {
                            FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.zoom), Brushes.LightPink);
                        }
                        else if (item.Childs.Contains(hovered))
                        {
                            FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.zoom), Brushes.LightBlue);
                        }
                        else
                        if (item == selected)
                        {
                            FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.zoom), Brushes.LightGreen);
                        }
                        else
                        {
                            Brush brush = Brushes.LightGray;

                            switch (item.LayerType)
                            {
                                case LayerType.Conv:
                                    brush = StaticColors.ConvBrush;
                                    textBrush = Brushes.White;
                                    break;
                                case LayerType.Batch:
                                    brush = StaticColors.BatchNormBrush;
                                    textBrush = Brushes.White;
                                    break;
                                case LayerType.Relu:
                                    brush = StaticColors.ReluBrush;
                                    textBrush = Brushes.White;
                                    break;
                                case LayerType.MathOperation:
                                    brush = StaticColors.AddBrush;
                                    textBrush = Brushes.White;
                                    break;
                                case LayerType.Concat:
                                    brush = StaticColors.ConcatBrush;
                                    textBrush = Brushes.White;
                                    break;
                            }
                            FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.zoom), brush);
                        }
                        DrawRoundedRectangle(ctx.Graphics, rr, (int)(40 * ctx.zoom), Pens.Black);


                        ctx.Graphics.ResetTransform();
                        var sh = ctx.Transform(dtag.Rect.Left, dtag.Rect.Top + 10);
                        ctx.Graphics.TranslateTransform(sh.X, sh.Y);
                        ctx.Graphics.ScaleTransform(ctx.zoom, ctx.zoom);
                        //ctx.Graphics.DrawString($"{item.Name}: ({item.OpType})", f, Brushes.Black, 0, 0);
                        var ms = ctx.Graphics.MeasureString(item.Name, f);
                        ctx.Graphics.DrawString(item.Name, f, textBrush, +dtag.Rect.Width / 2 - ms.Width / 2, 0);
                        var ms2 = ctx.Graphics.MeasureString(item.OpType, f);
                        ctx.Graphics.DrawString(item.OpType, f, textBrush, +dtag.Rect.Width / 2 - ms2.Width / 2, 30);
                        if (item.Parents.Contains(hovered))
                        {
                            ctx.Graphics.DrawString("child", f, textBrush, +dtag.Rect.Width / 2 - ms2.Width / 2, 60);
                        }
                        if (item.Childs.Contains(hovered))
                        {
                            ctx.Graphics.DrawString("parent", f, textBrush, +dtag.Rect.Width / 2 - ms2.Width / 2, 60);
                        }


                        ctx.Graphics.ResetTransform();
                    }

                }

                ctx.Box.Invoke((Action)(() =>
                {
                    ctx.Swap();
                    //ctx.Box.Refresh();
                }));
            }
        }

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



        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;

            LoadModel(ofd.FileName);

        }

        public const string WindowCaption = "Dendrite";

        public GraphModel Model;
        public bool LoadModel(string path)
        {
            var fr = Providers.FirstOrDefault(z => z.IsSuitableFile(path));
            if (fr == null)
            {
                MessageBox.Show("Unsupported file format.", WindowCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            var model = fr.LoadFromFile(path);
            Model = model;
            listView1.Items.Clear();

            foreach (var item in model.Nodes)
            {
                //listView1.Items.Add(new ListViewItem(new string[] { item.Name, ss, item.Output[0] }) { Tag = nodes[i] });
            }

            //var cnt2 = res2.Graph.Output[0].Name;
            //nodes.InsertRange(0, res2.Graph.Input.Select(z => outs[z.Name]));


            CurrentLayout();
            //Text = $"{WindowCaption}: {Path.GetFileName(path)}";
            if (ParentForm != null)
            {
                ParentForm.Text = Path.GetFileName(path);
            }
            return true;
        }

        public Action CurrentLayout;

        public void TableLayoutGraph()
        {
            var www = Model.Nodes.OrderBy(z => z.Parents.Count).Reverse().ToArray();
            www = Model.Nodes.ToArray();
            List<GraphNode> topo = new List<GraphNode>();
            List<GraphNode> visited = new List<GraphNode>();

            foreach (var item in www)
            {
                if (visited.Contains(item)) continue;
                dfs(item, topo, visited);
            }
            topo.Reverse();

            int cntr = 0;
            var s1 = (int)Math.Ceiling(Math.Sqrt(topo.Count));
            for (int i = 0; i < s1; i++)
            {
                for (int j = 0; j < s1; j++)
                {
                    if (cntr >= topo.Count) break;
                    topo[cntr].DrawTag = new GraphNodeDrawInfo() { Text = topo[cntr].Name, Rect = new Rectangle(i * 350, j * 150, 300, 100) };
                    cntr++;
                }
                if (cntr >= topo.Count) break;
            }
        }


        public void dfs(GraphNode node, List<GraphNode> outp, List<GraphNode> visited)
        {
            visited.Add(node);
            foreach (var item in node.Childs)
            {
                if (visited.Contains(item)) continue;
                dfs(item, outp, visited);
            }
            outp.Add(node);
        }

        public void LayoutGraph()
        {

            List<GraphNode> topo = new List<GraphNode>();
            List<GraphNode> visited = new List<GraphNode>();

            foreach (var item in Model.Nodes)
            {
                if (!visited.Contains(item))
                {
                    dfs(item, topo, visited);
                }
            }

            topo.Reverse();

            int yy = 100;
            int line = 0;
            /*   var inps = Graph.Where(z => z.Parent == null).ToArray();
               GraphNode crnt = inps[0];
               Queue<GraphNode> q = new Queue<GraphNode>();
               q.Enqueue(crnt);
               List<GraphNode> visited = new List<GraphNode>();
               while (q.Any())
               {
                   var deq = q.Dequeue();
                   if (visited.Contains(deq)) continue;
                   if (deq.DrawTag != null) continue;
                   if (deq.Parent == null)
                   {
                       deq.DrawTag = new GraphNodeDrawInfo() { Text = deq.Name, Rect = new Rectangle(100, 100, 300, 100) };
                   }
                   else
                   {
                       var dtag = deq.Parent.DrawTag as GraphNodeDrawInfo;
                       var ind = deq.Parent.Childs.IndexOf(deq);
                       var rect = dtag.Rect;
                       deq.DrawTag = new GraphNodeDrawInfo() { Text = deq.Name, Rect = new Rectangle(rect.X + ind * 350, rect.Bottom+50, rect.Width, rect.Height) };
                   }

                   foreach (var item in deq.Childs)
                   {                    
                       q.Enqueue(item);
                   }
               }*/
            /*while (true)
            {
                crnt.DrawTag = new GraphNodeDrawInfo() { Text = crnt.Name, Rect = new Rectangle(100, yy, 300, 100) };
                if (crnt.Childs.Count == 0) break;

                crnt = crnt.Childs[0];

                yy += 150;
            }*/
            /*return;
             * */
            foreach (var item in topo)
            {
                int shift = 0;
                int xx = 100;
                if (item.Parent != null)
                {
                    shift += item.Parent.Childs.IndexOf(item) * 350;
                    if (item.Parent.Childs.IndexOf(item) != 0)
                    {
                        line++;
                        shift = line * 350;
                    }
                    var info = (item.Parent.DrawTag as GraphNodeDrawInfo);
                    xx = info.Rect.Left + shift;
                    yy = info.Rect.Bottom + 50;
                }
                item.DrawTag = new GraphNodeDrawInfo() { Text = item.Name, Rect = new Rectangle(xx, yy, 300, 100) };
                yy += 150;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (Model == null) { MessageBox.Show("load model first", WindowCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK) return;
            Model.Provider.SaveModel(Model, sfd.FileName);

        }

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
            }
            else
            {
                HideInfoTab();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tableLayoutPanel1.RowStyles[0].Height = groupBox1.Height;
        }

        private void tableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLayout = TableLayoutGraph;
            TableLayoutGraph();
        }

        private void simpleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentLayout = LayoutGraph;
            CurrentLayout();
        }
    }
}
