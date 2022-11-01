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
            
            ConsoleInference c = new ConsoleInference(args[0]);
            var sw = Stopwatch.StartNew();
            var result = c.Inference(args[1]);
            sw.Stop();
            result.SaveImage(args[2]);
            
            System.Console.WriteLine($"Inference time: {sw.ElapsedMilliseconds}ms");
            System.Console.WriteLine($"Output saved to : {args[1]}");                
        }

        static void Usage()
        {
            System.Console.WriteLine("usage: [exe] <.den environment> <source image> <output image>");
        }

    }
}