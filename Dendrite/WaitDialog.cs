using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dendrite
{
    public partial class WaitDialog : Form
    {
        public WaitDialog()
        {
            InitializeComponent();
            Shown += WaitDialog_Shown;
            FormClosing += WaitDialog_FormClosing;
        }
        bool finished = false;
        private void WaitDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!finished)
                e.Cancel = true;
        }

        Action act;
        private void WaitDialog_Shown(object sender, EventArgs e)
        {
            Thread th = new Thread(() => { act(); finished = true; });            
            th.IsBackground = true;
            th.Start();
        }

        public void Init(Action act)
        {
            this.act = act;
        }

        int cnt = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (cnt == 100) { cnt = 0; }
            progressBar1.Value = cnt++;
            if (finished)
            {
                Close();
            }
        }
    }
}
