﻿using Dendrite.Lib;
using Google.Protobuf;
using Onnx;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Dendrite
{
    public partial class Edit : Form
    {
        public Edit()
        {
            InitializeComponent();
            var v = Enum.GetValues(typeof(AttributeProto.Types.AttributeType));
            comboBox1.Items.Clear();
            foreach (var item in v)
            {
                comboBox1.Items.Add(new ComboBoxItem() { Name = item.ToString(), Tag = item });
            }
        }

        NodeProto node;



        void updateList()
        {
            listView1.Items.Clear();
            listView2.Items.Clear();

            foreach (var item in node.Attribute)
            {
                string val = item.AsString();
                listView1.Items.Add(new ListViewItem(new string[] { item.Name, item.Type.ToString(), val }) { Tag = item });
            }

            int index = 0;
            foreach (var item in node.Input)
            {
                var fr = model.Graph.Initializer.FirstOrDefault(z => z.Name == item);
                if (fr == null)
                {
                    listView2.Items.Add(new ListViewItem(new string[] { index + "", item, string.Empty, string.Empty, }));
                }
                else
                {
                    listView2.Items.Add(new ListViewItem(new string[] { index + "", item, fr.DataType.ToString(), fr.CalculateSize().ToString() }) { Tag = item });
                }
                index++;
            }
        }

        ModelProto model;
        public void Init(ModelProto model, NodeProto proto, GraphNode gnode)
        {
            this.model = model;
            Text = "Edit node: " + proto.Name;
            this.node = proto;
            updateList();
            textBox3.Text = proto.Name;
            this.gnode = gnode;
        }
        GraphNode gnode;

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var attr = listView1.SelectedItems[0].Tag as AttributeProto;
            node.Attribute.Remove(attr);
            updateList();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            if ((listView1.SelectedItems[0].Tag is AttributeProto attr))
            {
                textBox1.Text = attr.Name;
                textBox2.Text = attr.AsString();
            }
            if ((listView1.SelectedItems[0].Tag is TensorProto tensor))
            {
                textBox1.Text = tensor.Name;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var attr = listView1.SelectedItems[0].Tag as AttributeProto;
            attr.Name = textBox1.Text;
            parseAndAssignValue(attr, textBox2.Text);
            updateList();
        }

        void parseAndAssignValue(AttributeProto attr, string val)
        {
            switch (attr.Type)
            {
                case AttributeProto.Types.AttributeType.Float:
                    attr.F = float.Parse(val.Replace(",", "."), CultureInfo.InvariantCulture);
                    break;
                case AttributeProto.Types.AttributeType.Int:
                    attr.I = int.Parse(val);
                    break;
                case AttributeProto.Types.AttributeType.String:

                    attr.S = ByteString.CopyFromUtf8(val);
                    break;

                case AttributeProto.Types.AttributeType.Floats:
                    {
                        attr.Floats.Clear();
                        var aa = val.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(z => float.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
                        foreach (var item in aa)
                        {
                            attr.Floats.Add(item);
                        }
                    }
                    break;
                case AttributeProto.Types.AttributeType.Ints:
                    {
                        attr.Ints.Clear();
                        var aa = val.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(z => int.Parse(z)).ToArray();
                        foreach (var item in aa)
                        {
                            attr.Ints.Add(item);
                        }
                    }
                    break;
                case AttributeProto.Types.AttributeType.Strings:
                    {
                        attr.Strings.Clear();
                        var aa = val.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(z => ByteString.CopyFromUtf8(z)).ToArray();
                        foreach (var item in aa)
                        {
                            attr.Strings.Add(item);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var aa = new AttributeProto() { Name = textBox1.Text };
            aa.Type = (AttributeProto.Types.AttributeType)(comboBox1.SelectedItem as ComboBoxItem).Tag;
            parseAndAssignValue(aa, textBox2.Text);
            node.Attribute.Add(aa);
            updateList();
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            var name = listView2.SelectedItems[0].Tag as string;
            var ind = node.Input.IndexOf(name);
            node.Input[ind] = string.Empty;
            updateList();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            node.Name = textBox3.Text;
            this.gnode.Name = node.Name;
        }
    }
}
