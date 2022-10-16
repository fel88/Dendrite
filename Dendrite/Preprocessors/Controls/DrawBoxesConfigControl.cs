using System;
using System.Windows.Forms;
using System.Globalization;

namespace Dendrite.Preprocessors.Controls
{
    public partial class DrawBoxesConfigControl : UserControl, IProcessorConfigControl
    {
        public DrawBoxesConfigControl()
        {
            InitializeComponent();
        }

        public DrawBoxesPostProcessor Proc;

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Proc.VisThreshold = float.Parse(textBox1.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {

            }

        }

        public void Init(IInputPreprocessor proc)
        {
            Proc = proc as DrawBoxesPostProcessor;
            textBox1.Text = Proc.VisThreshold.ToString();
            checkBox1.Checked = Proc.DrawLabels;
        }

        private void DrawBoxesConfigControl_Load(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Proc.DrawLabels = checkBox1.Checked;
        }
    }
}
