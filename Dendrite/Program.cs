using Dagre;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Dendrite
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new Mdi();

            //DagreInputGraph dg = new DagreInputGraph();
            //var nd1 = dg.AddNode(new { Name = "input" }, 100, 20);
            //var nd2 = dg.AddNode(new { Name = "node1" }, 150, 30);
            //var nd3 = dg.AddNode(new { Name = "node2" }, 150, 30);
            //var nd4 = dg.AddNode(new { Name = "output" }, 100, 20);
            //dg.AddEdge(nd1, nd2, 2);
            //dg.AddEdge(nd2, nd3);
            //dg.AddEdge(nd3, nd4, 2);
            //dg.Layout();

            //Console.WriteLine($"{((dynamic)nd1.Tag).Name} : {nd1.X} {nd1.Y}");
            //Console.WriteLine($"{((dynamic)nd2.Tag).Name} : {nd2.X} {nd2.Y}");
            //Console.WriteLine($"{((dynamic)nd3.Tag).Name} : {nd3.X} {nd3.Y}");
            //Console.WriteLine($"{((dynamic)nd4.Tag).Name} : {nd4.X} {nd4.Y}");

            //Bitmap bmp = new Bitmap(400, 600);
            //var gr = Graphics.FromImage(bmp);
            //gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //gr.Clear(Color.White);
            //int gap = 10;
            //foreach (var item in dg.Nodes())
            //{
            //    var rect = new RectangleF(item.X + gap, item.Y, item.Width, item.Height);
            //    gr.FillRectangle(Brushes.Blue, rect);
            //    gr.DrawRectangle(Pens.LightBlue, item.X + gap, item.Y, item.Width, item.Height);
            //    gr.DrawString(((dynamic)item.Tag).Name, SystemFonts.DefaultFont, Brushes.White, rect, new StringFormat() { Alignment = StringAlignment.Center });
            //}
            //bmp.Save("graph1.jpg");
            //BERTTester.UndoTest();
            //DagreTester.TestCluster();
            //DagreTester.TestCluster2();
            Application.Run(MainForm);
        }

        public static Mdi MainForm;
    }
}
