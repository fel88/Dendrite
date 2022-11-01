using Dendrite;
using OpenCvSharp;
using System.Diagnostics;
using System;

namespace Dendrite.Console 
{
    public class ConsoleInference
    {
        public ConsoleInference(string envPath)
        {            
            env.Load(envPath);
        }

        InferenceEnvironment env = new InferenceEnvironment();

        public Mat Inference(string path)
        {
            var topo = env.Pipeline.Toposort();
            if (topo.Length > 0 && topo.Any(z => z is ImageSourceNode))
            {
                var sn = topo.First(z => z is ImageSourceNode) as ImageSourceNode;
                sn.SourceMat = Cv2.ImRead(path);
                return Run();
            }
            return null;
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