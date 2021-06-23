using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Dendrite
{
    public partial class ArrayComparer : Form
    {
        public ArrayComparer()
        {
            InitializeComponent();
        }

        float[] arr1;
        float[] arr2;

        public void update2D()
        {
            listView2.Items.Clear();
            listView2.Columns.Clear();
            listView2.Columns.Add("offset");
            for (int i = 0; i < 16; i++)
            {
                listView2.Columns.Add("#" + i.ToString());
            }
            int cntr = 0;
            for (int i = offset; i < arr1.Length; i++, cntr++)
            {
                var ll = arr1.Skip(i * 16).Take(16).Select(z => z.ToString()).ToList();
                ll.Insert(0, (i * 16).ToString());
                listView2.Items.Add(new ListViewItem(ll.ToArray()) { });
                if (cntr > rows) break;
            }
        }
        public void update()
        {
            update2D();
            if (arr2 == null) return;
            double maxdiff = 0;
            int totalDiffs = 0;
            for (int i = offset; i < arr1.Length; i++)
            {
                var diff = Math.Abs(arr1[i] - arr2[i]);
                if (diff > eps)
                {
                    if (diff > maxdiff)
                    {
                        maxdiff = diff;
                    }
                    totalDiffs++;
                }
            }
            label5.Text = "max diff: " + maxdiff;
            toolStripStatusLabel1.Text = "total diffs: " + totalDiffs.ToString("N0");

            listView1.Items.Clear();
            int cntr = 0;
            for (int i = offset; i < arr1.Length; i++)
            {
                var lvi = new ListViewItem(new string[] { i.ToString(), arr1[i].ToString(), arr2[i].ToString() }) { };
                if (checkBox1.Checked)
                {
                    if (Math.Abs(arr1[i] - arr2[i]) > eps)
                    {
                        lvi.BackColor = Color.Yellow;
                        listView1.Items.Add(lvi);
                        cntr++;
                    }
                }
                else
                {
                    if (Math.Abs(arr1[i] - arr2[i]) > eps)
                    {
                        lvi.BackColor = Color.Yellow;

                    }
                    listView1.Items.Add(lvi);
                    cntr++;
                }
                if (cntr > rows) break;

            }
        }
        double eps = 10e-5;
        internal void Init(float[] vs, float[] inputData)
        {
            arr1 = vs;
            arr2 = inputData;
            label2.Text = "len1:" + arr1.Length.ToString("N0");
            if (inputData != null)
            {
                label3.Text = "len2: " + inputData.Length.ToString("N0");
            }

            update();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            update();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        int offset;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            offset = int.Parse(textBox1.Text);
        }

        int rows = 100;
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            rows = int.Parse(textBox3.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                eps = double.Parse(textBox2.Text);
                textBox2.BackColor = Color.White;
                textBox2.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                textBox2.BackColor = Color.Red;
                textBox2.ForeColor = Color.White;
            }
        }
    }
}
