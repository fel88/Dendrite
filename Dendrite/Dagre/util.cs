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
            var min = g.nodes.Min(z => z.rank);
            foreach (var item in g.nodes)
            {
                if (item.rank.HasValue)
                {
                    item.rank -= min;
                }
            }
        }

        public static int uniqueId()
        {
            uniqueCounter++;
            return uniqueCounter;
        }
        public static int uniqueCounter = 0;

        /*
 * Adds a dummy node to the graph and return v.
 */
        public static DagreNode addDummyNode(DagreGraph g, object type,  object attrs, string name)
        {
            DagreNode v=new DagreNode ();
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
            return g.nodes.Where(z => g.node(z.key).rank != null).Select(z => g.node(z.key).rank.Value).Max();

        }
        /*
 * Given a DAG with each node assigned "rank" and "order" properties, this
 * function will produce a matrix with the ids of each node.
 */
        public static Dictionary<string, DagreNode>[] buildLayerMatrix(DagreGraph g)
        {
            var range = Enumerable.Range(0, maxRank(g) + 1);
            List<Dictionary<string, DagreNode>> layering = range.Select(z => new Dictionary<string, DagreNode>()).ToList();
            //var layering = _.map(_.range(maxRank(g) + 1), function() { return []; });

            foreach (var v in g.nodes)
            {
                var node = g.node(v.key);
                var rank = node.rank;
                if (rank != null)
                {
                    layering[rank.Value].Add(node.order, v);
                }
            }

            return layering.ToArray();
        }

        internal static object addBorderNode(DagreGraph g, string v)
        {
            throw new NotImplementedException();
        }
    }
}
