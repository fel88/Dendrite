using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Onnx;

namespace Dendrite
{
    public partial class ExpandControl : UserControl
    {
        public ExpandControl()
        {
            InitializeComponent();
        }

        public void SetCaption(string text)
        {
            label1.Text = text;
        }
        public void SetValue(string text)
        {
            textBox1.Text = text;
        }

        public List<ExpandSubItem> SubItems = new List<ExpandSubItem>();

        public bool PlusVisible
        {
            get
            {
                return button1.Visible;
            }
            set
            {
                button1.Visible = value;
            }
        }
        public void AddSubItem(string text, string value, Func<string> fullValue = null)
        {
            var bb = new ExpandSubItem() { Name = text, Value = value };
            SubItems.Add(bb);
            Label lab1 = new Label();
            bb.Label = lab1;
            lab1.Text = text;
            Button btn = new Button() { Text = "S" };
            btn.Tag = fullValue == null ? value : (object)fullValue;
            btn.Click += Btn_Click;
            btn.Width = 30;


            TextBox tb1 = new TextBox();
            btn.Height = tb1.Height;


            bb.Tb = tb1;
            bb.Btn = btn;

            tb1.Text = value;
            tb1.ReadOnly = true;
            lab1.Left = 10;
            tb1.Left = lab1.Right + 10;
            btn.Left = Width - 30;
            lab1.Top = textBox1.Bottom + 10;
            tb1.Top = textBox1.Bottom + 10;


            btn.Top = textBox1.Bottom + 10;

            if (SubItems.Count > 1)
            {
                lab1.Top = SubItems[SubItems.Count - 2].Tb.Bottom + 10;
                tb1.Top = SubItems[SubItems.Count - 2].Tb.Bottom + 10;
                btn.Top = SubItems[SubItems.Count - 2].Tb.Bottom + 10;
            }

            Controls.Add(lab1);
            Controls.Add(tb1);
            Controls.Add(btn);
            btn.BringToFront();
            lab1.Visible = false;
            tb1.Visible = false;
            btn.Visible = false;
            //Height = SubItems.Max(z => z.Label.Bottom);

        }

        ITag model;

        GraphModel graph;
        internal void SetModel(GraphModel graph, ITag model)
        {
            this.graph = graph;
            this.model = model;
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Tag is string s)
            {
                Clipboard.SetText(s);
            }
            else
            {
                Clipboard.SetText(((sender as Button).Tag as Func<string>)());

            }
            MessageBox.Show("Data saved to clipboard!");
        }

        bool collapsed = true;
        public void Switch()
        {
            collapsed = !collapsed;
            foreach (var item in SubItems)
            {
                item.Label.Visible = !collapsed;
                item.Tb.Visible = !collapsed;
                item.Btn.Visible = !collapsed;
            }
            if (!collapsed)
            {
                Height = SubItems.Max(z => z.Label.Bottom);
            }
            else
            {
                Height = label1.Bottom;
            }
            PlusChanged?.Invoke();
        }

        public Action PlusChanged;
        private void button1_Click(object sender, EventArgs e)
        {
            if (SubItems.Count == 0)
            {
                if (model.Tag is ValueInfoProto vip)
                {
                    TextEnterDialog t = new TextEnterDialog();
                    t.Init(vip.Name);
                    t.ShowDialog();

                    var ww = (graph as OnnxGraphModel).ProtoModel.Graph.Node.Where(z => z.Output.Any(u => u == vip.Name)).ToArray();
                    foreach (var item in ww)
                    {
                        for (int i = 0; i < item.Output.Count; i++)
                        {
                            if (item.Output[i] == vip.Name)
                                item.Output[i] = t.DataText;
                        }
                    }
                    ww = (graph as OnnxGraphModel).ProtoModel.Graph.Node.Where(z => z.Input.Any(u => u == vip.Name)).ToArray();
                    foreach (var item in ww)
                    {
                        for (int i = 0; i < item.Input.Count; i++)
                        {
                            if (item.Input[i] == vip.Name)
                                item.Input[i] = t.DataText;
                        }
                    }
                    vip.Name = t.DataText;                    
                    
                }
                //model.Provider.UpdateIntAttributeValue(model, new GraphNode() { Name = nodeName }, attributeName, 0);
            }
            else
            {
                Switch();
            }
        }
    }
}
