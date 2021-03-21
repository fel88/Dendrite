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
    public partial class ZeroImageConfigControl : UserControl,IProcessorConfigControl
    {
        public ZeroImageConfigControl()
        {
            InitializeComponent();
        }

        ZeroImagePreprocessor proc;
        public void Init(IInputPreprocessor proc2)
        {
            this.proc = proc2 as ZeroImagePreprocessor;
            textBox1.Text = proc.Height.ToString();
            textBox2.Text = proc.Width.ToString();
            textBox3.Text = proc.Filler.ToString();
            radioButton1.Checked = proc.Channels == 3;
            radioButton2.Checked = proc.Channels == 1;

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            proc.Width = int.Parse(textBox2.Text);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            proc.Height = int.Parse(textBox1.Text);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            proc.Filler = byte.Parse(textBox3.Text);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            
            if(radioButton1.Checked) proc.Channels = 3;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked) proc.Channels = 1;

        }
    }

}
