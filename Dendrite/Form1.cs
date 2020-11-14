using Onnx;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Dendrite
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ctx.Init(pictureBox1);
            pictureBox1.SetDoubleBuffered(true);
            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            ctx.Redraw = Redraw;


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

            Shown += Form1_Shown;
            pictureBox1.Focus();
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
        }

        ExpandGroupControl group1 = new ExpandGroupControl();
        ExpandGroupControl group2 = new ExpandGroupControl();
        ExpandGroupControl group3 = new ExpandGroupControl();
        ExpandGroupControl group4 = new ExpandGroupControl();

        GraphNode selected = null;

        public void UpdateInfo()
        {
            group1.Clear();
            group1.AddItem("type", selected.OpType);
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
                res.AddSubItem("data", item.Weights.Aggregate("", (x, y) => x + y + ","));
                res.PlusVisible = true;
                //item.Dims.Aggregate("", (x, y) => x + y + ", ")
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

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (hovered == null) return;
            selected = hovered;
            UpdateInfo();
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            pictureBox1.Focus();
        }
        public static void DrawRoundedRectangle(Graphics g,
                                 Rectangle r, int d, Pen pen)
        {
            g.DrawPath(pen, GetRoundedRectangle(r, d));
        }
        public static GraphicsPath GetRoundedRectangle(Rectangle r, int d)
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

        public static void FillRoundedRectangle(Graphics g,
                                Rectangle r, int d, Brush myBrush)
        {
            g.FillPath(myBrush, GetRoundedRectangle(r, d));
        }
        GraphNode hovered = null;
        Font f = new Font("Arial", 18);
        public object lock1 = new object();
        void Redraw()
        {
            lock (lock1)
            {
                ctx.Update();

                ctx.Graphics.Clear(Color.White);
                ctx.Graphics.SmoothingMode = SmoothingMode.AntiAlias;


                ctx.Graphics.ResetTransform();

                ///axis
                //ctx.Graphics.DrawLine(Pens.Red, ctx.Transform(new PointF(0, 0)), ctx.Transform(new PointF(1000, 0)));
                //ctx.Graphics.DrawLine(Pens.Blue, ctx.Transform(new PointF(0, 0)), ctx.Transform(new PointF(0, 1000)));

                foreach (var item in Graph)
                {
                    var dtag = item.DrawTag as GraphNodeDrawInfo;
                    foreach (var citem in item.Childs)
                    {
                        var dtag2 = citem.DrawTag as GraphNodeDrawInfo;
                        ctx.Graphics.DrawLine(Pens.Black,
                            ctx.Transform(dtag.Rect.Location.X + dtag.Rect.Size.Width / 2, dtag.Rect.Location.Y + dtag.Rect.Height / 2),
                            ctx.Transform(dtag2.Rect.Location.X + dtag2.Rect.Size.Width / 2, dtag2.Rect.Location.Y + dtag2.Rect.Height / 2)
                            );
                    }
                }


                foreach (var item in Graph)
                {
                    var dtag = item.DrawTag as GraphNodeDrawInfo;

                    var rr = ctx.Transform(dtag.Rect);
                    var rr2 = ctx.Transform(new Rectangle(dtag.Rect.Left, dtag.Rect.Top, dtag.Rect.Width, dtag.Rect.Height));


                    if (item == hovered)
                    {
                        FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.zoom), Brushes.LightYellow);
                    }
                    else
                    if (item == selected)
                    {
                        FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.zoom), Brushes.LightGreen);
                    }
                    else
                    {
                        FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.zoom), Brushes.LightBlue);
                    }
                    DrawRoundedRectangle(ctx.Graphics, rr, (int)(40 * ctx.zoom), Pens.Black);


                    ctx.Graphics.ResetTransform();
                    var sh = ctx.Transform(dtag.Rect.Left, dtag.Rect.Top + 10);
                    ctx.Graphics.TranslateTransform(sh.X, sh.Y);
                    ctx.Graphics.ScaleTransform(ctx.zoom, ctx.zoom);
                    //ctx.Graphics.DrawString($"{item.Name}: ({item.OpType})", f, Brushes.Black, 0, 0);
                    ctx.Graphics.DrawString(item.Name, f, Brushes.Black, 0, 0);
                    ctx.Graphics.DrawString(item.OpType, f, Brushes.Black, 0, 30);

                    ctx.Graphics.ResetTransform();
                }



                ctx.Box.Invoke((Action)(() => { ctx.Box.Refresh(); }));
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Thread th = new Thread(() =>
            {
                while (true)
                {
                    Redraw();
                    Thread.Sleep(15);
                }
            });
            th.IsBackground = true;
            th.Start();
        }

        GraphNode[] Graph = new GraphNode[] { };
        DrawingContext ctx = new DrawingContext();
        Onnx.ModelProto model;


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var bb = File.ReadAllBytes(ofd.FileName);
            var res2 = Onnx.ModelProto.Parser.ParseFrom(bb);
            model = res2;



            var nodes = new List<GraphNode>();
            nodes.AddRange(res2.Graph.Node.Select(z =>
            new GraphNode()
            {
                OpType = z.OpType,
                Name = z.Name,
                Tag = z
            }).ToArray());

            Dictionary<string, GraphNode> outs = new Dictionary<string, GraphNode>();
            foreach (var item in res2.Graph.Output)
            {
                var gn = new GraphNode() { Name = item.Name };
                //   nodes.Add(gn);
                //  outs.Add(gn.Name, gn);
            }
            foreach (var item in res2.Graph.Input)
            {
                outs.Add(item.Name, new GraphNode() { Name = item.Name });
            }

            for (int i = 0; i < res2.Graph.Node.Count; i++)
            {
                Onnx.NodeProto item = res2.Graph.Node[i];
                foreach (var item2 in item.Output)
                {
                    outs.Add(item2, nodes[i]);
                }
            }

            Dictionary<string, TensorProto> inits = new Dictionary<string, TensorProto>();
            foreach (var iitem in res2.Graph.Initializer)
            {
                inits.Add(iitem.Name, iitem);
            }
            for (int i = 0; i < res2.Graph.Node.Count; i++)
            {
                Onnx.NodeProto item = res2.Graph.Node[i];


                string ss = "";
                foreach (var aitem in item.Attribute)
                {
                    var atr1 = new AttributeInfo() { Name = aitem.Name };
                    nodes[i].Attributes.Add(atr1);
                    List<int[]> dd = new List<int[]>();
                    switch (aitem.Type)
                    {
                        case Onnx.AttributeProto.Types.AttributeType.Ints:
                            atr1.Type = AttributeInfoDataType.Ints;
                            atr1.Ints = aitem.Ints.ToList();
                            break;
                        case Onnx.AttributeProto.Types.AttributeType.Float:
                            atr1.Type = AttributeInfoDataType.Float32;
                            atr1.FloatData = aitem.F;
                            break;
                        case Onnx.AttributeProto.Types.AttributeType.Int:
                            atr1.Type = AttributeInfoDataType.Int;
                            atr1.IntData = aitem.I;
                            break;
                        case Onnx.AttributeProto.Types.AttributeType.String:
                            atr1.Type = AttributeInfoDataType.String;
                            atr1.StringData = aitem.S.ToStringUtf8();
                            break;
                        case AttributeProto.Types.AttributeType.Tensor:
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                }

                foreach (var iitem in item.Input)
                {
                    if (!outs.ContainsKey(iitem))
                    {
                        var id = new InputData() { Name = iitem };
                        nodes[i].Data.Add(id);
                        var initer = inits[iitem];

                        var bts = initer.RawData.ToByteArray();
                        List<float> ret = new List<float>();
                        for (int j = 0; j < bts.Length; j += 4)
                        {
                            ret.Add(BitConverter.ToSingle(bts, j));
                        }
                        id.Weights = ret.ToArray();
                        id.Dims = initer.Dims.ToArray();
                        continue;
                    }
                    nodes[i].Input = iitem;
                    nodes[i].Parent = outs[iitem];
                    outs[iitem].Childs.Add(nodes[i]);
                }
                if (item.Input.Any())
                {
                    ss = item.Input[0];

                }
                else
                {

                }

                if (!item.Output.Any())
                {

                }


                listView1.Items.Add(new ListViewItem(new string[] { item.Name, ss, item.Output[0] }) { Tag = nodes[i] });

            }

            var cnt2 = res2.Graph.Output[0].Name;
            nodes.InsertRange(0, res2.Graph.Input.Select(z => outs[z.Name]));

            Graph = nodes.ToArray();
            LayoutGraph();
        }

        public void LayoutGraph()
        {
            int yy = 100;

            foreach (var item in Graph)
            {
                int shift = 0;
                int xx = 100;
                if (item.Parent != null)
                {
                    shift += item.Parent.Childs.IndexOf(item) * 350;
                    var info = (item.Parent.DrawTag as GraphNodeDrawInfo);
                    xx = info.Rect.Left + shift;
                    yy = info.Rect.Bottom + 20;
                }
                item.DrawTag = new GraphNodeDrawInfo() { Text = item.Name, Rect = new Rectangle(xx, yy, 300, 100) };
                yy += 120;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (model == null) { MessageBox.Show("load model first"); return; }
            //res2.Graph.Input[0].Name = "inputz";
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK) return;
            using (var output = File.Create(sfd.FileName))
            {
                using (Google.Protobuf.CodedOutputStream ff = new Google.Protobuf.CodedOutputStream(output))
                {
                    model.WriteTo(ff);
                }
            }
            return;
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

            hovered = null;
            foreach (var item in Graph)
            {
                var dtag = item.DrawTag as GraphNodeDrawInfo;
                var rr = ctx.Transform(dtag.Rect);
                var rr1 = GetRoundedRectangle(rr, (int)(40 * ctx.zoom));
                if (rr1.IsVisible(pos))
                {
                    hovered = item;
                    break;
                }
            }
        }

        public void UpdateSearch()
        {
            var gg = Graph.Where(z => z.Name.ToLower().Contains(textBox1.Text.ToLower()));
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
    }
}
