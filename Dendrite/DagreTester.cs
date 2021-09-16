using Dendrite.Dagre;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Dendrite
{
    public static class DagreTester
    {
        public static string ReadResourceTxt(string resourceName)
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


            if (!DagreGraph.FromJson(ReadResourceTxt("beforeRank.txt")).Compare(ncg)) throw new DagreException("wrong");
            dl.rank(ncg);



            if (!DagreGraph.FromJson(ReadResourceTxt("beforeInjectEdgeLabelProxies.txt")).Compare(dg)) throw new DagreException("wrong");
            DagreLayout.injectEdgeLabelProxies(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("beforeRemoveEmptyRanks.txt")).Compare(dg)) throw new DagreException("wrong");
            DagreLayout.removeEmptyRanks(dg);
            nestingGraph.cleanup(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterCleanup.txt")).Compare(dg)) throw new DagreException("wrong");

            util.normalizeRanks(dg);


        }
        public static void Test2()
        {
            var dl = new DagreLayout();
            var dg = DagreGraph.FromJson(ReadResourceTxt("beforeAsNoneCompoundGraph.txt"));
            //DagreGraph ncg = DagreGraph.FromJson(ReadResourceTxt("beforeRank.txt"));
            var ncg = util.asNonCompoundGraph(dg);

            dl.rank(ncg);

            if (!DagreGraph.FromJson(ReadResourceTxt("beforeInjectEdgeLabelProxies.txt")).Compare(dg)) throw new DagreException("wrong");
            util.uniqueCounter = 1;
            DagreLayout.injectEdgeLabelProxies(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("beforeRemoveEmptyRanks.txt")).Compare(dg)) throw new DagreException("wrong");
            DagreLayout.removeEmptyRanks(dg);

            nestingGraph.cleanup(dg);

            util.normalizeRanks(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterNormalizeRanks.txt")).Compare(dg)) throw new DagreException("wrong");

            dl.assignRankMinMax(dg);

            dl.removeEdgeLabelProxies(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("beforeNormalize.txt")).Compare(dg)) throw new DagreException("wrong");

            normalize.run(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterNormalize.txt")).Compare(dg)) throw new DagreException("wrong");

        }
    }
}
