using OpenCvSharp;
using System;
using System.Diagnostics;

namespace Dendrite.Console // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Usage();
                return;
            }
            
            //OpenCvSharp.log::utils::logging::setLogLevel(cv::utils::logging::LogLevel::LOG_LEVEL_SILENT);
            ConsoleInference c = new ConsoleInference(args[0]);
            var sw = Stopwatch.StartNew();
            string[] img_exts = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
            string[] vid_exts = new[] { ".avi", ".mkv", ".mp4" };
            if (img_exts.Contains(Path.GetExtension(args[1]).ToLower()))
            {
                var result = c.Inference(args[1]);
                result.SaveImage(args[2]);
            }
            else if (vid_exts.Contains(Path.GetExtension(args[1]).ToLower()))
            {
                c.InferenceVideo(args[1], args[2]);
            }
            sw.Stop();

            System.Console.WriteLine($"Inference time: {sw.ElapsedMilliseconds}ms");
            System.Console.WriteLine($"Output saved to : {args[1]}");
        }

        static void Usage()
        {
            System.Console.WriteLine("usage: [exe] <.den environment> <source image> <output image>");
        }
    }
}