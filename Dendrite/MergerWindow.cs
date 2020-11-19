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
    public partial class MergerWindow : Form
    {
        public MergerWindow()
        {
            InitializeComponent();
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            foreach (var item in Program.MainForm.MdiChildren)
            {
                foreach (var citem in (item as Form).Controls)
                {
                    if (citem is Form1 f1)
                    {
                        if (f1.Model != null)
                        {
                            comboBox1.Items.Add(new ComboBoxItem() { Name = f1.Model.Name, Tag = f1.Model });
                        }

                    }
                }
            }
        }

        public class ComboBoxItem
        {
            public string Name { get; set; }
            public object Tag;
            public override string ToString()
            {
                return Name;
            }
        }

        GraphModel model1;
        GraphModel model2;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(comboBox1.SelectedItem is ComboBoxItem cbi)) return;

            var gm = cbi.Tag as GraphModel;
            model1 = gm;
            UpdateList(gm, listView1, textBox1.Text);
        }

        public void UpdateList(GraphModel gm, ListView listView1, string mask)
        {
            listView1.Items.Clear();
            mask = mask.ToLower();
            foreach (var item in gm.Nodes)
            {
                foreach (var ccc in item.Data)
                {
                    var c1 = $"{item.Name}:{ccc.Name}";
                    var c2 = string.Join(", ", ccc.Dims);
                    if (!string.IsNullOrEmpty(mask))
                    {
                        if (!(c1.ToLower().Contains(mask) || c2.ToLower().Contains(mask))) continue;
                    }
                    listView1.Items.Add(new ListViewItem(new string[] { c1, c2 }) { Tag = ccc });
                }
            }
            toolStripStatusLabel1.Text = $"{listView1.Items.Count} loaded";
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(comboBox2.SelectedItem is ComboBoxItem cbi)) return;

            var gm = cbi.Tag as GraphModel;
            model2 = gm;
            UpdateList(gm, listView2, textBox2.Text);



        }

        private void comboBox2_DropDown(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            foreach (var item in Program.MainForm.MdiChildren)
            {
                foreach (var citem in (item as Form).Controls)
                {
                    if (citem is Form1 f1)
                    {
                        if (f1.Model != null)
                        {
                            comboBox2.Items.Add(new ComboBoxItem() { Name = f1.Model.Name, Tag = f1.Model });
                        }

                    }
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            UpdateList(model1, listView1, textBox1.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            UpdateList(model2, listView2, textBox2.Text);

        }

        private void assignSelectedBothSideToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
