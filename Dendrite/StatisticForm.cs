using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using OpenCvSharp.Extensions;
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
    public partial class StatisticForm : Form
    {
        public StatisticForm()
        {
            InitializeComponent();
        }
        public void Init(string lastPath)
        {
            var session1 = new InferenceSession(lastPath);
            foreach (var item in session1.OutputMetadata.Keys)
            {
                var dims = session1.OutputMetadata[item].Dimensions;
                _nodes.Add(new NodeInfo() { Name = item, Dims = dims });
            }

            foreach (var name in session1.InputMetadata.Keys)
            {
                var dims = session1.InputMetadata[name].Dimensions;
                var s1 = string.Join("x", dims);
                _nodes.Add(new NodeInfo() { Name = name, Dims = dims, IsInput = true });

            }
        }
        List<NodeInfo> _nodes = new List<NodeInfo>();
        string path;
        OpenCvSharp.VideoCapture cap;
        string netPath;
        string inputName;
        int[] inputDims;
        IInputPreprocessor[] preps;
        public void Init(string p, string netPath, string inputName, int[] inputDims, IInputPreprocessor[] preps)
        {
            this.preps = preps;
            this.inputName = inputName;
            this.inputDims = inputDims;
            this.netPath = netPath;
            Init(netPath);
            var nd = _nodes.First(z => z.IsInput);
            this.inputName = nd.Name;
            this.inputDims = nd.Dims;
            //InputDatas[nd.Name].Preprocessors.ToArray()
            path = p;
            label3.Text = path;


        }
        Thread th;
        bool stop = false;
        private void button1_Click(object sender, EventArgs e)
        {

            if (th != null)
            {
                button1.Text = "start";
                stop = true;
                th = null;
                return;
            }
            button1.Text = "stop";
            th = new Thread(() =>
            {
                cap = new OpenCvSharp.VideoCapture(path);
                Stopwatch sw = Stopwatch.StartNew();
                var session1 = new InferenceSession(netPath);

                var inputMeta = session1.InputMetadata;


                Mat mat = new Mat();
                var nFrames = cap.Get(VideoCaptureProperties.FrameCount);
                cap.Read(mat);
                var sz = mat.Size();
                if (inputDims[2] == -1)
                {
                    sz.Height = mat.Height;
                    sz.Width = mat.Width;
                }
                string key = $"{sz.Width}x{sz.Height}";
                if (!Processing.allPriorBoxes.ContainsKey(key))
                {
                    var pd = Decoders.PriorBoxes2(sz.Width, sz.Height); ;
                    Processing.allPriorBoxes.Add(key, pd);
                }
                var prior_data = Processing.allPriorBoxes[key];
                var ofps = cap.Get(VideoCaptureProperties.Fps);
                VideoWriter vid = null;
                if (checkBox1.Checked)
                {
                    vid = new VideoWriter("output.mp4", FourCC.XVID, ofps, mat.Size());
                }
                while (true)
                {
                    if (stop) break;
                    var pf = cap.Get(VideoCaptureProperties.PosFrames);
                    int perc = (int)Math.Round((pf / (float)nFrames) * 100);
                    progressBar1.Invoke(((Action)(() =>
                    {
                        label1.Text = $"{pf} / {nFrames}  {perc}%";
                        progressBar1.Value = perc;
                    })));
                    if (!cap.Read(mat))
                    {
                        break;
                    }
                    Mat orig = mat.Clone();
                    if (inputDims[2] == -1)
                    {
                        inputDims[2] = mat.Height;
                        inputDims[3] = mat.Width;
                    }

                    mat.ConvertTo(mat, MatType.CV_32F);
                    object param = mat;
                    foreach (var pitem in preps)
                    {
                        param = pitem.Process(param);
                    }

                    var inputData = param as float[];
                    var tensor = new DenseTensor<float>(param as float[], inputDims);
                    var container = new List<NamedOnnxValue>();

                    container.Add(NamedOnnxValue.CreateFromTensor<float>(inputName, tensor));

                    float[] confd;
                    float[] locd;
                    using (var results = session1.Run(container))
                    {
                        var data = results.First().AsTensor<float>();
                        locd = data.ToArray();
                        confd = results.Skip(1).First().AsTensor<float>().ToArray();
                    }


                    Stopwatch sw2 = Stopwatch.StartNew();
                    var ret = Processing.boxesDecode(orig.Size(), confd, locd, new System.Drawing.Size(sz.Width, sz.Height), prior_data, visTresh);
                    if (checkBox1.Checked)
                    {
                        var out1 = Processing.drawBoxes(orig, ret.Item1, ret.Item2, visTresh, ret.Item3);
                        vid.Write(out1);
                    }
                    sw2.Stop();
                }
                vid.Release();



                sw.Stop();

            });
            th.IsBackground = true;
            th.Start();
        }
        float visTresh = 0.5f;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                visTresh = float.Parse(textBox1.Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            path = ofd.FileName;
            label3.Text = path;
        }
    }
}
