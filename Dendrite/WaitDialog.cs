using System;
using System.Threading;
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
        public Exception Exception;
        private void WaitDialog_Shown(object sender, EventArgs e)
        {
            Thread th = new Thread(() => {
               // try
                {
                    act();
                }
              //  catch (Exception ex)
                {
               //     Exception = ex;
                }
               // finally
                {
                    finished = true;
                }
            
            });            
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
