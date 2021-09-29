using System.IO;
using System.Windows.Forms;
using Dagre;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System;
using System.Threading;

namespace Dendrite
{
    public class BERTTester
    {
        public static void UndoTest()
        {
            OpenFileDialog ofd = new OpenFileDialog();


            if (ofd.ShowDialog() != DialogResult.OK) return;

            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.MaxJsonLength = Int32.MaxValue;
            var des = jss.Deserialize<dynamic>(File.ReadAllText(ofd.FileName));


            var dg = DagreGraph.FromJson(des);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Thread.Sleep(1000);
            normalize.undo(dg);
            sw.Stop();
            var ms = sw.ElapsedMilliseconds;

        }
    }
}
