﻿using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics;

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

        async void Run()
        {
            try
            {
                var sw = Stopwatch.StartNew();
                toolStrip1.Enabled = false;
                await Task.Run(() =>
                {

                    env.Process();
                    var outps = env.Pipeline.GetOutputs();

                    if (outps.Any(z => z is Mat))
                    {
                        var m = outps.OfType<Mat>().First();
                        m.ConvertTo(m, MatType.CV_8UC3);
                        pictureBox2.Image = m.ToBitmap();
                    }
                    sw.Stop();

                });
                toolStripStatusLabel1.Text = $"inference time: {sw.ElapsedMilliseconds}ms";
                toolStripStatusLabel1.BackColor = SystemColors.Control;
                toolStripStatusLabel1.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = ex.Message + ex.StackTrace;
                toolStripStatusLabel1.BackColor = Color.Red;
                toolStripStatusLabel1.ForeColor = Color.White;
            }
            finally
            {
                toolStrip1.Enabled = true;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null) 
                return;

            pictureBox1.Image.Save("temp1.jpg");            
            Process.Start(new ProcessStartInfo("temp1.jpg") { UseShellExecute = true });
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null)
                return;

            pictureBox2.Image.Save("temp2.jpg");                        
            Process.Start(new ProcessStartInfo("temp2.jpg") { UseShellExecute = true });            
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            var topo = env.Pipeline.Toposort();
            if (topo.Length > 0 && topo.Any(z => z is ImageSourceNode))
            {
                var sn = topo.First(z => z is ImageSourceNode) as ImageSourceNode;
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
