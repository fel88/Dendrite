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
            path = p;
            cap = new OpenCvSharp.VideoCapture(path);

        }
        Thread th;
        private void button1_Click(object sender, EventArgs e)
        {

            if (th != null) return;
            th = new Thread(() =>
            {
                Stopwatch sw = Stopwatch.StartNew();
                var session1 = new InferenceSession(netPath);

                var inputMeta = session1.InputMetadata;
                var container = new List<NamedOnnxValue>();

                Mat mat2 = null;
                Mat mat = new Mat();
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
                while (true)
                {

                    if (!cap.Read(mat))
                    {
                        break;
                    }

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

                    container.Add(NamedOnnxValue.CreateFromTensor<float>(inputName, tensor));
                }

                float[] confd;
                float[] locd;
                using (var results = session1.Run(container))
                {



                    var data = results.First().AsTensor<float>();
                    locd = data.ToArray();
                    confd = results.Skip(1).First().AsTensor<float>().ToArray();


                }


                Stopwatch sw2 = Stopwatch.StartNew();
                var ret = Processing.boxesDecode(mat2, confd, locd, new System.Drawing.Size(sz.Width, sz.Height), prior_data);
                sw2.Stop();

                sw.Stop();

            });
        }
    }
}
