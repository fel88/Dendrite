using Dendrite.Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dendrite.Preprocessors.Controls
{
    public partial class PriorBoxesConfigControl : UserControl, IProcessorConfigControl
    {
        public PriorBoxesConfigControl()
        {
            InitializeComponent();
        }
        PriorBoxesProcessor proc;

        public void Init(IInputPreprocessor proc2)
        {
            proc = proc2 as PriorBoxesProcessor;
            Sync();
        }
        void Sync()
        {
            checkBox1.Checked = proc.PriorBoxes2Mode;

            textBox12.Text = string.Join(", ", proc.Variances.Select(Helpers.ToDoubleInvariantString));
            textBox11.Text = string.Join(", ", proc.Steps);
            textBox10.Text = string.Join(", ", proc.MinSizes.Select(z => $"[{string.Join(",", z)}]"));
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            proc.MinSizes = new int[][] { new int[] { 16, 32 }, new int[] { 64, 128 }, new int[] { 256, 512 } };
            proc.Steps = new int[] { 8, 16, 32 };
            proc.PriorBoxes2Mode = false;
            Sync();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            proc.MinSizes = new int[][] { new int[] { 32, 64, 128 }, new int[] { 256 }, new int[] { 512 } };
            proc.Steps = new int[] { 32, 64, 128 };
            proc.PriorBoxes2Mode = true;
            Sync();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            proc.PriorBoxes2Mode = checkBox1.Checked;
        }
    }
}
