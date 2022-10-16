using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dendrite
{
    public partial class Inference : Form
    {
        public Inference()
        {
            InitializeComponent();
        }

        InferenceEnvironment env;

        public void Init(InferenceEnvironment _env)
        {
            env = _env;
            Text = env.Path;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            pictureBox1.Image = Bitmap.FromFile(ofd.FileName);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            var sw = Stopwatch.StartNew();
            env.Process();
            sw.Stop();
            toolStripStatusLabel1.Text = $"inference time: {sw.ElapsedMilliseconds}ms";       
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {            
            pictureBox1.Image.Save("temp1.jpg");
            Process.Start("temp1.jpg");
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            pictureBox2.Image.Save("temp2.jpg");
            Process.Start("temp2.jpg");
        }
    }
}
