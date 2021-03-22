using System;
using System.Windows.Forms;
using System.Globalization;

namespace Dendrite.Preprocessors.Controls
{
    public partial class DrawBoxesConfigControl : UserControl,IProcessorConfigControl
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
        }
    }
}
