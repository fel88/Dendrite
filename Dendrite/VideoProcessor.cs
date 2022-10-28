using Dendrite.Preprocessors;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dendrite
{
    public partial class VideoProcessor : Form
    {
        public VideoProcessor()
        {
            InitializeComponent();
            Shown += VideoProcessor_Shown;
        }

        public int OutputVideoFps = 25;
        string outputPath;
        private void VideoProcessor_Shown(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "MP4 video (*.mp4)|*.mp4";
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                Close();
                return;
            }
            outputPath = sfd.FileName;

            th.Start();
        }

        InferenceEnvironment env;

        Thread th;
        internal void Init(InferenceEnvironment _env, string fileName)
        {
            env = _env;

            //var node = env.Net.Nodes.First(z => z.IsInput);
            VideoCapture cap = new VideoCapture(fileName);
            Mat mat = new Mat();
            cap.Read(mat);
            var topo = env.Pipeline.Toposort();
            if (topo.Length == 0 || !(topo.Any(z => z is ImageSourceNode))) return;
            var sn = (topo.First(z=> z is ImageSourceNode) as ImageSourceNode);

            Text = $"Processing: {fileName}  {mat.Width}x{mat.Height}";
            sn.SourceMat = mat.Clone();            
            env.Process();
            var outp1 = env.Pipeline.GetOutputs();
            var last1 = outp1.First(z => z is Mat) as Mat;

            /*if (env.Net.InputDatas.ContainsKey(node.Name) && env.Net.InputDatas[node.Name] is InputInfo ii)
            {
                ii.Data = mat;
            }
            else
            {
                env.Net.InputDatas[node.Name] = new InputInfo() { Data = mat };
            }*/


            th = new Thread(() =>
           {
               try
               {
                   using (var vid = new VideoWriter(outputPath, FourCC.XVID, OutputVideoFps, new OpenCvSharp.Size(last1.Width, last1.Height)))
                   {
                       var nFrames = cap.Get(VideoCaptureProperties.FrameCount);


                       Mat img = new Mat();
                       while (cap.Read(img))
                       {
                           if (cancel)
                               break;

                           sn.SourceMat = img.Clone();
                           env.Process();
                           var outp = env.Pipeline.GetOutputs();
                           var last = outp.First(z => z is Mat) as Mat;
                           vid.Write(last);
                           var pf = cap.Get(VideoCaptureProperties.PosFrames);
                           int perc = (int)Math.Round((pf / (float)nFrames) * 100);
                           progressBar1.Invoke(((Action)(() =>
                           {
                               Text = $"Processing: {fileName}  {mat.Width}x{mat.Height}  {pf} / {nFrames}  {perc}%";
                               progressBar1.Value = perc;
                           })));
                       }
                       progressBar1.Invoke(((Action)(() =>
                       {
                           if (Helpers.ShowQuestion("Open video?", Text) == DialogResult.Yes)
                           {
                               Process.Start(outputPath);
                           }
                       })));

                   }
               }
               finally
               {
                   Close();
               }
           });
            th.IsBackground = true;

        }
        bool cancel = false;
        private void button1_Click(object sender, EventArgs e)
        {
            cancel = true;
        }
    }
}
