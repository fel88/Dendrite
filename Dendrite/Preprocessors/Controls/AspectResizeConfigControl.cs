using System;
using System.Windows.Forms;

namespace Dendrite.Preprocessors.Controls
{
    public partial class AspectResizeConfigControl : UserControl, IProcessorConfigControl
    {
        public AspectResizeConfigControl()
        {
            InitializeComponent();
        }

        public AspectResizePreprocessor proc;

        public void Init(IInputPreprocessor proc2)
        {
            this.proc = proc2 as AspectResizePreprocessor;
            checkBox1.Checked = proc.ForceH;
            textBox1.Text = proc.H.ToString();

        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            proc.ForceH = checkBox1.Checked;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                proc.H = int.Parse(textBox1.Text);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
