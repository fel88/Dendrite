using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace Dendrite.Console // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Debugger.Launch();
            if (args.Length < 2 && !System.Console.IsInputRedirected)
            {
                Usage();
                return;
            }
            Mat pipeInput = null;
            if (System.Console.IsInputRedirected)
            {

                System.Console.InputEncoding = Encoding.UTF8;
                StringBuilder sb = new StringBuilder();

                using (StreamReader reader = new StreamReader(System.Console.OpenStandardInput(), System.Console.InputEncoding))
                {
                    string stdin;
                    while ((stdin = reader.ReadLine()) != null)
                    {
                        if (stdin.Trim().StartsWith("#"))
                            continue;

                        sb.AppendLine(stdin.Trim());
                    }
                }                

                pipeInput = ParsePPM(sb.ToString());
            }

            //OpenCvSharp.log::utils::logging::setLogLevel(cv::utils::logging::LogLevel::LOG_LEVEL_SILENT);
            ConsoleInference c = new ConsoleInference(args[0]);
            var sw = Stopwatch.StartNew();
            string[] img_exts = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".ppm" };
            string[] vid_exts = new[] { ".avi", ".mkv", ".mp4" };
            if (pipeInput != null || img_exts.Contains(Path.GetExtension(args[1]).ToLower()))
            {
                if (pipeInput == null)
                {
                    pipeInput = Cv2.ImRead(args[1]);
                }
                var result = c.Inference(pipeInput);
                if (args.Length < 3)
                {
                    //raw ppm output
                    System.Console.WriteLine($"P3");//ASCII PPM format
                    System.Console.WriteLine($"{result.Width} {result.Height}");
                    System.Console.WriteLine($"255");
                    var bmp = result.ToBitmap();
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        for (int i = 0; i < bmp.Width; i++)
                        {
                            var p = bmp.GetPixel(i, j);
                            System.Console.WriteLine($"{p.R} {p.G} {p.B}");
                        }
                    }
                    bmp.Dispose();
                    return;
                }
                else
                    result.SaveImage(args[2]);
            }
            else if (vid_exts.Contains(Path.GetExtension(args[1]).ToLower()))
            {
                c.InferenceVideo(args[1], args[2]);
            }
            else
            {
                System.Console.WriteLine($"unsupported format");
            }
            sw.Stop();

            System.Console.WriteLine($"Inference time: {sw.ElapsedMilliseconds}ms");

            if (args.Length > 2 && File.Exists(args[2]))
                System.Console.WriteLine($"Output saved to : {args[2]}");
        }

        private static Mat? ParsePPM(string v)
        {
            var reader = new StringReader(v);
            string str;
            var header = reader.ReadLine();
            var sizes = reader.ReadLine().Split(' ');
            int w = int.Parse(sizes[0]);
            int h = int.Parse(sizes[1]);
            int max = int.Parse(reader.ReadLine());
            Bitmap bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            for (int j = 0; j < h; j++)
            {
                for (int i = 0; i < w; i++)
                {
                    var temp = reader.ReadLine().Trim();
                    var rgb = temp.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                    var clr = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
                    bmp.SetPixel(i, j, clr);
                }
            }
            var mat = bmp.ToMat();
            bmp.Dispose();
            return mat;

        }

        static void Usage()
        {
            System.Console.WriteLine("usage: [exe] <.den environment> <source image> <output image>");
        }
    }
}