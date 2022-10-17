using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            // var fr = net.Nodes.First(z => z.IsInput);
            //  LoadImage();
        }

        private bool LoadImage(ImageSourceNode sn)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();

                if (ofd.ShowDialog() != DialogResult.OK) return false;
                // lastPath = ofd.FileName;

                if (ofd.FileName.EndsWith("mp4") || ofd.FileName.EndsWith("avi") || ofd.FileName.EndsWith("mkv"))
                {
                    VideoCapture cap = new VideoCapture(ofd.FileName);
                    Mat mat = new Mat();
                    cap.Read(mat);


                    Text = $"Processing: {ofd.FileName}  {mat.Width}x{mat.Height}";

                    /*if (InputDatas.ContainsKey(node.Name) && InputDatas[node.Name] is InputInfo ii)
                    {
                        ii.Data = cap;
                    }
                    else
                    {
                        InputDatas[node.Name] = new InputInfo() { Data = cap };
                    }*/
                    sn.SourceMat = mat.Clone();

                    pictureBox1.Image = BitmapConverter.ToBitmap(mat);
                }
                else
                {
                    var mat = OpenCvSharp.Cv2.ImRead(ofd.FileName);
                    Text = $"Processing: {ofd.FileName}  {mat.Width}x{mat.Height}";
                    //mat.ConvertTo(mat, MatType.CV_32F);
                    /*
                                        if (InputDatas.ContainsKey(node.Name) && InputDatas[node.Name] is InputInfo ii)
                                        {
                                            ii.Data = mat;
                                        }
                                        else
                                        {
                                            InputDatas[node.Name] = new InputInfo() { Data = mat };
                                        }*/
                    sn.SourceMat = mat.Clone();

                    pictureBox1.Image = BitmapConverter.ToBitmap(mat);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return true;
        }

        Nnet net => env.Net;

        public Dictionary<string, InputInfo> InputDatas => net.InputDatas;
        public Dictionary<string, object> OutputDatas => net.OutputDatas;

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Run();
        }

        void Run()
        {
            var sw = Stopwatch.StartNew();
            env.Process();
            var outps = env.Pipeline.GetOutputs();

            if (outps[0] is Mat m)
            {
                pictureBox2.Image = m.ToBitmap();
            }
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

        private void toolStripButton3_Click(object sender, EventArgs e)
        {            
            var topo = env.Pipeline.Toposort();
            if (topo.Any() && topo[0] is ImageSourceNode sn)
            {
                if (LoadImage(sn))                
                    Run();                
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() != DialogResult.OK) return;
            // lastPath = ofd.FileName;
            VideoProcessor vp = new VideoProcessor();

            if (ofd.FileName.EndsWith("mp4") || ofd.FileName.EndsWith("avi") || ofd.FileName.EndsWith("mkv"))
            {
                vp.Init(env, ofd.FileName);
                vp.ShowDialog();

            }
        }
    }
}
