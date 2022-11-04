using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Dendrite.Preprocessors.Controls
{
    [PreprocessorBind(typeof(DrawInstanceSegmentationPostProcessor))]
    public partial class InstanceSegmentatorDrawerConfigControl : UserControl, IProcessorConfigControl
    {
        public InstanceSegmentatorDrawerConfigControl()
        {
            InitializeComponent();
        }

        public DrawInstanceSegmentationPostProcessor Proc;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Proc.EnableBoxDraw = checkBox1.Checked;
        }

        public void Init(IInputPreprocessor proc)
        {
            Proc = proc as DrawInstanceSegmentationPostProcessor;
            checkBox1.Checked = Proc.EnableBoxDraw;
            checkBox2.Checked = Proc.EnableTextDraw;
            
            listView1.Items.Clear();
            if (Proc.LastDetections != null)
                foreach (var item in Proc.LastDetections)
                {
                    listView1.Items.Add(new ListViewItem(new string[] { "" }) { Tag = item });
                }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Proc.EnableTextDraw = checkBox2.Checked;
        }

        private void showhideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var tag = listView1.SelectedItems[0].Tag as SegmentationDetectionInfo;
            tag.Visible = !tag.Visible;
            //Proc.Redraw();
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            if (Proc.LastDetections != null)
                foreach (var item in Proc.LastDetections)
                {
                    listView1.Items.Add(new ListViewItem(new string[] { item.Label, item.Conf + "" }) { Tag = item });
                }
        }

        private void saveMaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var tag = listView1.SelectedItems[0].Tag as SegmentationDetectionInfo;
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK) return;
            tag.Mask.SaveImage(sfd.FileName);
        }
        /*
        private void nmsFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Proc.Detections == null) return;
            var bb = Proc.Detections.Select(z => new float[] { z.Rect.X, z.Rect.Y, z.Rect.X + z.Rect.Width, z.Rect.Y + z.Rect.Height, z.Conf }).ToArray();
            var ff = Decoders.nms(bb.ToList(), Proc.NmsThreshold);
            List<SegmentationDetectionInfo> ss = new List<SegmentationDetectionInfo>();
            for (int i = 0; i < ff.Length; i++)
            {
                ss.Add(Proc.Detections[ff[i]]);
            }
            Proc.Detections = ss.ToArray();
            updateToolStripMenuItem_Click(null, null);
            Proc.Redraw();
        }*/

        /*private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Proc.NmsThreshold = float.Parse(textBox1.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {

            }
        }*/
    }
}
