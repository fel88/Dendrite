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
                    if (ccc.Dims == null) continue;
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
            foreach (var item in listView3.Items)
            {
                if (!((item as ListViewItem).Tag is Tuple<InputData, InputData> t)) continue;

                model1.Provider.UpdateFloatTensor(model1, t.Item1.Parent, t.Item1.Name, t.Item2.Weights, t.Item2.Dims);
            }
            MessageBox.Show("Done!", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (listView1.SelectedItems.Count == 0) return;
            if (listView2.SelectedItems.Count == 0) return;
            if (listView1.SelectedItems.Count != listView2.SelectedItems.Count)
            {
                MessageBox.Show("left count != right count", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int k = 0; k < listView1.SelectedItems.Count; k++)
            {
                var id1 = listView1.SelectedItems[k].Tag as InputData;
                var id2 = listView2.SelectedItems[k].Tag as InputData;
                /*if (id1.Dims.Length != id2.Dims.Length)
                {
                    MessageBox.Show("dims match error", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }*/
                if (id1.Weights.Length != id2.Weights.Length)
                {
                    MessageBox.Show("values sizes match error", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                /*for (int i = 0; i < id1.Dims.Length; i++)
                {
                    if (id1.Dims[i] != id2.Dims[i])
                    {
                        MessageBox.Show("dims match error", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }*/

                var lvi = AddMatchItem(id1, id2);


            }

        }

        ListViewItem AddMatchItem(InputData id1, InputData id2)
        {
            var lvi = new ListViewItem(new string[] { $"{model1.Name}:{id1.Parent.Name}:{id1.Name}", string.Join(",", id1.Dims), $"{model2.Name}:{id2.Parent.Name}:{id2.Name}", string.Join(",", id2.Dims) }) { Tag = new Tuple<InputData, InputData>(id1, id2) };
            listView3.Items.Add(lvi);
            bool same = true;
            for (int i = 0; i < id1.Weights.Length; i++)
            {
                if (id1.Weights[i] != id2.Weights[i])
                {
                    same = false;
                    break;
                }
            }
            if (same)
            {
                lvi.BackColor = Color.Green;
                lvi.ForeColor = Color.White;
            }
            return lvi;
        }

        ListViewItem AddNotMatchItem(InputData id1)
        {
            var lvi = new ListViewItem(new string[] { $"{model1.Name}:{id1.Parent.Name}:{id1.Name}", string.Join(",", id1.Dims), "", "" }) { Tag = id1 };
            listView3.Items.Add(lvi);
            lvi.BackColor = Color.LightYellow;
            return lvi;
        }



        GraphNode[] GetShortestPath(GraphNode start, GraphNode target, GraphModel model)
        {

            Dictionary<long, GraphNode> dic1 = new Dictionary<long, GraphNode>();
            foreach (var item in model.Nodes)
            {
                dic1.Add(item.Id, item);
            }
            List<GraphNode> path = new List<GraphNode>();
            Dictionary<long, long> p = new Dictionary<long, long>();
            HashSet<long> used = new HashSet<long>();
            Queue<GraphNode> q = new Queue<GraphNode>();
            used.Add(start.Id);
            p.Add(start.Id, -1);
            q.Enqueue(start);
            while (q.Any())
            {
                var deq = q.Dequeue();
                foreach (var item in deq.Childs)
                {
                    if (used.Contains(item.Id)) continue;
                    used.Add(item.Id);
                    q.Enqueue(item);
                    if (!p.ContainsKey(item.Id)) { p.Add(item.Id, 0); }
                    p[item.Id] = deq.Id;
                }

            }
            //restore path
            for (long i = target.Id; i != -1; i = p[i])
                path.Add(dic1[i]);
            path.Reverse();


            return path.ToArray();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (model1 == null) return;
            if (model2 == null) return;

            listView3.Items.Clear();
            Dictionary<long, List<InputData>> d2 = new Dictionary<long, List<InputData>>();



            foreach (var item in model2.Nodes)
            {
                foreach (var ditem in item.Data)
                {
                    var sz = ditem.Dims.Aggregate((long)1, (x, y) => x * y);
                    if (!d2.ContainsKey(sz))
                    {
                        d2.Add(sz, new List<InputData>());
                    }
                    d2[sz].Add(ditem);
                }
            }
            List<InputData> used = new List<InputData>();

            int notMatched = 0;
            foreach (var item in model1.Nodes)
            {
                foreach (var ditem in item.Data)
                {
                    var sz = ditem.Dims.Aggregate((long)1, (x, y) => x * y);
                    if (!d2.ContainsKey(sz))
                    {
                        notMatched++;
                        AddNotMatchItem(ditem);
                        continue;
                    }
                    var cnd = d2[sz].Where(z => z.Parent.LayerType == item.LayerType).ToArray();

                    List<InputData> toDel = new List<InputData>();
                    foreach (var ccitem in cnd)
                    {
                        if (ccitem.Dims.Length != ditem.Dims.Length) continue;
                        for (int j = 0; j < ccitem.Dims.Length; j++)
                        {
                            if (ccitem.Dims[j] != ditem.Dims[j])
                            {
                                toDel.Add(ccitem);
                                break;
                            }
                        }
                    }
                    cnd = cnd.Except(toDel).ToArray();


                    if (cnd.Length == 1)
                    {
                        if (used.Contains(cnd[0]))
                        {
                            notMatched++;
                            AddNotMatchItem(ditem);
                            continue;
                        }
                        AddMatchItem(ditem, cnd[0]);

                        used.Add(cnd[0]);
                        continue;
                    }


                    var path1 = GetShortestPath(model1.Nodes.First(z => z.LayerType == LayerType.Input), item, model1);
                    StringBuilder sb = new StringBuilder();
                    foreach (var crn in path1)
                    {
                        if (crn.LayerType == LayerType.Batch || crn.LayerType == LayerType.Conv || crn.LayerType == LayerType.Relu)
                            sb.Append(crn.LayerType + ";");
                    }

                    sb.Append(":" + item.Data.IndexOf(ditem));


                    bool was = false;

                    foreach (var citem in cnd)
                    {

                        var path2 = GetShortestPath(model2.Nodes.First(z => z.LayerType == LayerType.Input), citem.Parent, model2);
                        StringBuilder sb1 = new StringBuilder();
                        foreach (var crn in path2)
                        {
                            if (crn.LayerType == LayerType.Batch || crn.LayerType == LayerType.Conv || crn.LayerType == LayerType.Relu)
                                sb1.Append(crn.LayerType + ";");
                        }


                        sb1.Append(":" + citem.Parent.Data.IndexOf(citem));
                        if (sb1.ToString() == sb.ToString())
                        {
                            if (used.Contains(citem))
                                continue;

                            AddMatchItem(ditem, citem);
                            used.Add(citem);
                            was = true;
                            break;
                        }

                    }
                    if (was) continue;
                    notMatched++;
                    AddNotMatchItem(ditem);
                }
            }
            if (notMatched > 0)
            {
                MessageBox.Show("not matched: " + notMatched);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView3.SelectedItems.Count == 0) return;
            List<ListViewItem> tt = new List<ListViewItem>();
            foreach (var item in listView3.SelectedItems)
            {
                tt.Add(item as ListViewItem);
            }

            foreach (var item in tt)
            {
                listView3.Items.Remove(item);
            }
        }
    }
}

