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
    public partial class Mdi : Form
    {
        public Mdi()
        {
            InitializeComponent();
            /*var f1 = new Form1();
            var frm = GenerateChildForm(f1);
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();*/

        }



        public const string WindowCaption = "Dendrite";
        public Form GenerateChildForm(Control cntr)
        {
            var frm = new Form();
            frm.Size = new Size(700, 500);
            //frm.MinimumSize = new Size(700, 500);

            frm.Controls.Add(cntr);
            cntr.Dock = DockStyle.Fill;

            frm.MdiParent = this;

            frm.FormClosing += (x, y) =>
            {
                foreach (var item in frm.Controls)
                {
                    if (item is Form1 f)
                    {
                        f.StopDrawThread();
                    }
                }
            };
            frm.Shown += (x, y) =>
            {
                foreach (var item in frm.Controls)
                {
                    if (item is Form1 f)
                    {
                        f.StartDrawThread();
                    }
                }

            };
            return frm;
        }

        private void multidocumentToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void singleToolStripMenuItem_Click(object sender, EventArgs e)
        {


        }

        private void Mdi_DragDrop(object sender, DragEventArgs e)
        {
            var ar = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (ar == null || ar.Length < 1) return;
            LoadModel(ar[0]);

        }

        void LoadModel(string ar)
        {
            //if (IsMdiContainer)
            {
                var f1 = new Form1();
                var frm = GenerateChildForm(f1);
                if (!f1.LoadModel(ar))
                {
                    return;
                }
                if (MdiChildren.Length == 1)
                {
                    frm.WindowState = FormWindowState.Maximized;
                }
                frm.Show();

            }
            /* else
             {
                 foreach (var item in Controls)
                 {
                     if (item is Form1 uc)
                     {
                         uc.LoadModel(ar);
                     }
                 }
             }*/
        }



        private void Mdi_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            LoadModel(ofd.FileName);
        }

        private void tileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void verticaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);

        }

        private void cascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);

        }



        private void mergeWeightsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MergerWindow w = new MergerWindow();
            w.MdiParent = this;
            w.Show();
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var item in MdiChildren)
            {
                item.Close();
            }
        }

        private void windowsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var t = sender as ToolStripMenuItem;
            t.DropDownItems.Clear();
            foreach (var item in MdiChildren)
            {
                var t1 = new ToolStripMenuItem() { Tag = item, Text = item.Text };
                t.DropDownItems.Add(t1);
                t1.Click += T1_Click;
            }
        }

        private void T1_Click(object sender, EventArgs e)
        {
            ((sender as ToolStripItem).Tag as Form).Activate();
        }

        private void modelToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void inferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "onnx|*.onnx";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            Processing p = new Processing();
            p.MdiParent = this;
            p.Init(ofd.FileName);
            p.Show();
        }

        private void gatherStatisticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "onnx|*.onnx";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            StatisticForm s = new StatisticForm();

            s.Init(ofd.FileName);
            s.Init(null, ofd.FileName, null, null, null);
            s.MdiParent = this;
            s.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
