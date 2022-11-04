using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dendrite
{
    public partial class NetControlConfig : UserControl
    {
        public NetControlConfig()
        {
            InitializeComponent();

        }
        public void Init(Nnet net)
        {
            this.net = net;
            UpdateNodesList(net);
        }

        void UpdateNodesList(Nnet net)
        {
            listView1.Items.Clear();
            listView2.Items.Clear();

            foreach (var item in net.Nodes.Where(z => !z.IsInput))
            {
                var dims = item.Dims;
                listView1.Items.Add(new ListViewItem(new string[] { item.Name, string.Join("x", dims),item.ElementType.Name, "" }) { Tag = item });
            }

            foreach (var item in net.Nodes.Where(z => z.IsInput))
            {
                var dims = item.Dims;
                var s1 = string.Join("x", dims);
                listView2.Items.Add(new ListViewItem(new string[] { item.Name, s1, item.ElementType.Name }) { Tag = item });
            }
        }
        Nnet net;
        public Dictionary<string, InputInfo> InputDatas => net.InputDatas;
        public Dictionary<string, object> OutputDatas => net.OutputDatas;
        NodeInfo currentNode = null;
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
            /*if (OutputDatas[currentNode.Name] is Mat mat)
            {
                pictureBox1.Image = BitmapConverter.ToBitmap(mat);
            }*/
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            currentNode = (NodeInfo)((listView2.SelectedItems[0] as ListViewItem).Tag);

            if (InputDatas.ContainsKey(currentNode.Name))
            {
                //if (InputDatas[currentNode.Name].Data is Mat mat)
                {
                    //pictureBox1.Image = BitmapConverter.ToBitmap(mat);
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
        long offset = 0;
        private void button4_Click(object sender, EventArgs e)
        {
            if (currentNode == null) return;
            offset = long.Parse(textBox9.Text);
            listView4.Items.Clear();
            float[] farr;
            InternalArray iar = null;
            if (currentNode.IsInput)
            {
                farr = InputDatas[currentNode.Name].Data as float[];
            }
            else
            {
                farr = OutputDatas[currentNode.Name] as float[];
                iar = OutputDatas[currentNode.Name] as InternalArray;
            }

            if (farr != null)
            {
                for (int j = 0; j < 20; j++)
                {
                    listView4.Items.Add(new ListViewItem(new string[] { (j + offset).ToString("X2"), farr[j + offset].ToString() }) { Tag = (long)(j + offset) });
                }
            }
            if (iar != null)
            {
                for (int j = 0; j < 20; j++)
                {
                    listView4.Items.Add(new ListViewItem(new string[] { (j + offset).ToString("X2"), iar.Data[j + offset].ToString() }) { Tag = (long)(j + offset) });
                }
            }

        }
    }
}
