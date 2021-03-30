using System;
using System.Windows.Forms;
using System.Globalization;

namespace Dendrite.Preprocessors.Controls
{
    public partial class NmsConfigControl : UserControl, IProcessorConfigControl
    {
        public NmsConfigControl()
        {
            InitializeComponent();
        }

        NmsPostProcessors Proc;
        public void Init(IInputPreprocessor proc)
        {
            Proc = proc as NmsPostProcessors;
            textBox1.Text = Proc.NmsThreshold.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Proc.NmsThreshold = float.Parse(textBox1.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
