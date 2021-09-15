using Dendrite.Dagre;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Dendrite
{
    public static class DagreTester
    {
        static string ReadResourceTxt(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fr1 = assembly.GetManifestResourceNames().First(z => z.Contains(resourceName));

            using (Stream stream = assembly.GetManifestResourceStream(fr1))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static void Test1()
        {
            var dl = new DagreLayout();
            DagreGraph dg = DagreGraph.FromJson(ReadResourceTxt("json.txt"));

            dl.makeSpaceForEdgeLabels(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterMakeSpaceForEdgeLabels.txt")).Compare(dg)) throw new DagreException("wrong");
            dl.removeSelfEdges(dg);
            acyclic.run(dg);

            if (!DagreGraph.FromJson(ReadResourceTxt("beforeNestingRun.txt")).Compare(dg)) throw new DagreException("wrong");

            nestingGraph.run(dg);

            if (!DagreGraph.FromJson(ReadResourceTxt("beforeAsNoneCompoundGraph.txt")).Compare(dg)) throw new DagreException("wrong");

            var ncg = util.asNonCompoundGraph(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("beforeRank.txt")).Compare(dg)) throw new DagreException("wrong");

            dl.rank(ncg);

            if (!DagreGraph.FromJson(ReadResourceTxt("beforeInjectEdgeLabelProxies.txt")).Compare(dg)) throw new DagreException("wrong");
            DagreLayout.injectEdgeLabelProxies(dg);

        }
    }
}
