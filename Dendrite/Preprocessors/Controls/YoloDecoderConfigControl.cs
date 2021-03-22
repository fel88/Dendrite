using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace Dendrite.Preprocessors.Controls
{
    public partial class YoloDecoderConfigControl : UserControl, IProcessorConfigControl
    {

        public YoloDecodePreprocessor Proc;
        public YoloDecoderConfigControl()
        {
            InitializeComponent();
        }

        public void Init(IInputPreprocessor proc)
        {
            Proc = proc as YoloDecodePreprocessor;
            textBox1.Text = Proc.NmsThreshold.ToString();
            textBox2.Text = Proc.Threshold.ToString();
            textBox3.Text = string.Join(Environment.NewLine, Proc.AllowedClasses);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Proc.NmsThreshold = float.Parse(textBox1.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                textBox1.BackColor = Color.White;
            }
            catch (Exception ex)
            {
                textBox1.BackColor = Color.Red;

            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Proc.Threshold = float.Parse(textBox2.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                textBox2.BackColor = Color.White;
            }
            catch (Exception ex)
            {
                textBox2.BackColor = Color.Red;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Proc.AllowedClasses = textBox3.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
