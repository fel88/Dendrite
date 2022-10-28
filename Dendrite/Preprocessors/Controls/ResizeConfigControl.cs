using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dendrite.Preprocessors.Controls
{
    public partial class ResizeConfigControl : UserControl, IProcessorConfigControl
    {
        public ResizeConfigControl()
        {
            InitializeComponent();
        }

        ResizePreprocessor proc;
        public void Init(IInputPreprocessor proc2)
        {
            this.proc = proc2 as ResizePreprocessor;
            textBox1.Text = proc.Dims[3].ToString();
            textBox2.Text = proc.Dims[2].ToString();
            checkBox1.Checked = proc.InputSlots.Length == 2;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            proc.Dims[3] = int.Parse(textBox1.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            proc.Dims[2] = int.Parse(textBox2.Text);

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            var aa = proc.InputSlots.ToArray();
            if (checkBox1.Checked)
            {
                if (proc.InputSlots.Length != 2)
                {
                    proc.InputSlots = new DataSlot[2];
                    proc.InputSlots[0] = aa[0];
                    proc.InputSlots[1] = new DataSlot() { Name = "size" };
                    proc.Invalidate();
                }
            }
            else
            {
                if (proc.InputSlots.Length != 1)
                {
                    proc.InputSlots = new DataSlot[1];
                    proc.InputSlots[0] = aa[0];
                    proc.Invalidate();
                }
            }

        }
    }
}
