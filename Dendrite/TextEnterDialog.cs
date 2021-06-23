using System;
using System.Windows.Forms;

namespace Dendrite
{
    public partial class TextEnterDialog : Form
    {
        public TextEnterDialog()
        {
            InitializeComponent();
        }

        public string DataText
        {
            get;set;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            DataText = textBox1.Text;
            Close();
        }

        internal void Init(string name)
        {
            textBox1.Text = name;
        }
    }
}
