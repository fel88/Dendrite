using Dendrite.Preprocessors;
using OpenCvSharp;
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

            var node = env.Net.Nodes.First(z => z.IsInput);
            VideoCapture cap = new VideoCapture(fileName);
            Mat mat = new Mat();
            cap.Read(mat);


            Text = $"Processing: {fileName}  {mat.Width}x{mat.Height}";

            if (env.Net.InputDatas.ContainsKey(node.Name) && env.Net.InputDatas[node.Name] is InputInfo ii)
            {
                ii.Data = mat;
            }
            else
            {
                env.Net.InputDatas[node.Name] = new InputInfo() { Data = mat };
            }

            th = new Thread(() =>
           {
               try
               {
                   using (var vid = new VideoWriter(outputPath, FourCC.XVID, OutputVideoFps, new OpenCvSharp.Size(mat.Width, mat.Height)))
                   {
                       var nFrames = cap.Get(VideoCaptureProperties.FrameCount);


                       Mat img = new Mat();
                       while (cap.Read(img))
                       {
                           if (cancel) 
                               break;

                           (env.Net.InputDatas[node.Name] as InputInfo).Data = img;
                           env.Process();
                           
                           var last = env.Pipeline.Nodes.First(z => z.Outputs[0].OutputLinks.Count == 0 && z.Tag is IInputPreprocessor) as IInputPreprocessor;                           
                           if (last.OutputSlots[0].Data is Mat mt)
                           {
                               vid.Write(mt);
                           }
                           var pf = cap.Get(VideoCaptureProperties.PosFrames);
                           int perc = (int)Math.Round((pf / (float)nFrames) * 100);
                           progressBar1.Invoke(((Action)(() =>
                           {
                               Text = $"Processing: {fileName}  {mat.Width}x{mat.Height}  {pf} / {nFrames}  {perc}%";
                               progressBar1.Value = perc;
                           })));
                       }
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
