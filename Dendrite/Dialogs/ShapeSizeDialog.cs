using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dendrite.Dialogs
{
    public partial class ShapeSizeDialog : Form
    {
        public ShapeSizeDialog()
        {
            InitializeComponent();
        }
        private NodeInfo _node;
        public void Init(NodeInfo ar, bool disablePositives = true)
        {
            _node = ar;
            for (int i = 0; i < ar.Dims.Length; i++)
            {
                TextBox tb = new TextBox() { Tag = i };
                tb.Text = ar.Dims[i].ToString();
                tb.TextChanged += Tb_TextChanged;
                tb.Top = 10;
                tb.Width = 50;
                tb.Height = 30;
                Controls.Add(tb);
                tb.Left = 10 + i * 60;
                if (ar.Dims[i] > 0 && disablePositives)
                {
                    tb.Enabled = false;
                }
            }
        }

        private void Tb_TextChanged(object sender, EventArgs e)
        {
            var tb = (sender as TextBox);
            try
            {
                var val = int.Parse(tb.Text);

                var index = (int)tb.Tag;
                _node.Dims[index] = val;
                tb.BackColor = Color.White;
                tb.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                tb.BackColor = Color.Red;
                tb.ForeColor = Color.White;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
