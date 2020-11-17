using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dendrite.Dagre
{
    class util
    {

        /*
         * Adjusts the ranks for all nodes in the graph such that all nodes v have
         * rank(v) >= 0 and at least one node w has rank(w) = 0.
         */
        public static void normalizeRanks(DagreGraph g)
        {
            var min = g.nodes().Min(z => g.node(z).rank);
            foreach (var v in g.nodes())
            {
                var node = g.node(v);
                if (node.rank.HasValue)
                {
                    node.rank -= min;
                }
            }
        }


        public static int[] range(int start, int end)
        {
            return Enumerable.Range(start, end - start).ToArray();
        }
        public static int uniqueId(string str)
        {
            uniqueCounter++;
            return uniqueCounter;
        }
        public static int uniqueCounter = 0;

        /*
 * Adds a dummy node to the graph and return v.
 */
        public static DagreNode addDummyNode(DagreGraph g, object type, object attrs, string name)
        {
            DagreNode v = new DagreNode();
            throw new NotImplementedException();
            do
            {
                //   v = _.uniqueId(name);
            } while (g.hasNode(v));

            //attrs.dummy = type;
            //  g.setNode(v, attrs);
            return v;
        }

        public static int maxRank(DagreGraph g)
        {
            return g.nodes().Where(z => g.node(z).rank != null).Select(z => g.node(z).rank.Value).Max();

        }

        internal static string[][] cloneDeep(string[][] layering)
        {
            List<List<string>> ss = new List<List<string>>();
            foreach (var item in layering)

            {
                ss.Add(new List<string>());
                foreach (var ii in item)
                {
                    ss.Last().Add(ii);
                }
            }
            return ss.Select(z => z.ToArray()).ToArray();
        }

        /*
* Given a DAG with each node assigned "rank" and "order" properties, this
* function will produce a matrix with the ids of each node.
*/
        public static string[][] buildLayerMatrix(DagreGraph g)
        {
            var range = Enumerable.Range(0, maxRank(g) + 1);
            List<List<string>> layering = new List<List<string>>();
            foreach (var item in Enumerable.Range(0, maxRank(g) + 1))
            {
                layering.Add(new List<string>());
            }
            //var layering = _.map(_.range(maxRank(g) + 1), function() { return []; });

            foreach (var v in g.nodes())
            {
                var node = g.node(v);
                var rank = node.rank;
                if (rank != null)
                {
                    while (layering[rank.Value].Count < node.order)
                    {
                        layering[rank.Value].Add(null);
                    }
                    layering[rank.Value][node.order] = v;
                }
            }



            return layering.Select(z => z.ToArray()).ToArray();
        }

        internal static object addBorderNode(DagreGraph g, string v)
        {
            throw new NotImplementedException();
        }

        internal static int[] range(int v1, int v2, int step)
        {
            List<int> ret = new List<int>();
            for (int i = v1; i != v2; i += step)
            {
                ret.Add(i);
            }
            return ret.ToArray();
        }
    }
}
