using OpenCvSharp;

namespace Dendrite.Console
{
    public class ConsoleInference
    {
        public ConsoleInference(string envPath)
        {
            env.Load(envPath);
        }

        InferenceEnvironment env = new InferenceEnvironment();
        public Mat Inference(Mat mat)
        {
            var topo = env.Pipeline.Toposort();
            if (topo.Length > 0 && topo.Any(z => z is ImageSourceNode))
            {
                var sn = topo.First(z => z is ImageSourceNode) as ImageSourceNode;
                sn.SourceMat = mat;
                return Run();
            }
            return null;
        }

        public Mat Inference(string path)
        {
            return Inference(Cv2.ImRead(path));            
        }

        public void InferenceVideo(string path, string outputPath)
        {
            var topo = env.Pipeline.Toposort();
            if (!(topo.Length > 0 && topo.Any(z => z is ImageSourceNode))) return;

            var sn = topo.First(z => z is ImageSourceNode) as ImageSourceNode;
            VideoCapture cap = new VideoCapture(path);
            Mat mat = new Mat();
            cap.Read(mat);
            System.Console.WriteLine($"Processing: {path}  {mat.Width}x{mat.Height}");
            sn.SourceMat = mat.Clone();
            env.Process();
            var outp1 = env.Pipeline.GetOutputs();

            var last1 = outp1.First(z => z is Mat) as Mat;
            var nFrames = cap.Get(VideoCaptureProperties.FrameCount);
            var fps = cap.Get(VideoCaptureProperties.Fps);
            System.Console.WriteLine($"Input video FPS: {fps}");

            //int OutputVideoFps = 25;

            using (var progress = new ProgressBar())
            {
                using (var vid = new VideoWriter(outputPath, FourCC.XVID, fps, new OpenCvSharp.Size(last1.Width, last1.Height)))
                {
                    Mat img = new Mat();
                    while (cap.Read(img))
                    {
                        sn.SourceMat = img.Clone();
                        env.Process();
                        var outp = env.Pipeline.GetOutputs();
                        var last = outp.First(z => z is Mat) as Mat;
                        vid.Write(last);
                        var pf = cap.Get(VideoCaptureProperties.PosFrames);
                        var perc = (pf / (float)nFrames);
                        //if (perc > 0.1) break;
                        progress.Report(perc);

                    }
                }
            }

        }

        Mat Run()
        {
            try
            {
                env.Process();
                var outps = env.Pipeline.GetOutputs();

                if (outps.Any(z => z is Mat))
                {
                    var m = outps.OfType<Mat>().First();
                    m.ConvertTo(m, MatType.CV_8UC3);
                    Mat? ret = m;
                    return ret;
                }

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message + ex.StackTrace);
            }
            finally
            {

            }
            return null;
        }
    }
}