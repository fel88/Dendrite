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

        public static DagreGraph asNonCompoundGraph(DagreGraph g)
        {
            var simplified = new DagreGraph() { multigraph = g.isMultigraph() }.setGraph(g.graph());
            foreach (var v in g.nodes())
            {
                if (g.children(v).Length > 0)
                {
                    simplified.setNode(v, g.node(v));
                }
            }

            foreach (var e in g.edges())
            {
                simplified.setEdge(e, g.edge(e));

            }


            return simplified;
        }

        public static int[] range(int start, int end)
        {
            return Enumerable.Range(start, end - start).ToArray();
        }
        public static string uniqueId(string str)
        {
            uniqueCounter++;
            return str + uniqueCounter;
        }
        public static int uniqueCounter = 0;

        /*
 * Adds a dummy node to the graph and return v.
 */
        public static string addDummyNode(DagreGraph g, string type, object attrs, string name)
        {
            string v = null;

            do
            {
                v = uniqueId(name);
            } while (g.hasNode(v));

            //attrs.dummy = type;
            g.setNode(v, new DagreNode() { dummy = type });
            return v;
        }

        public static int maxRank(DagreGraph g)
        {
            return g.nodes().Where(z => g.node(z).rank != null).Select(z => g.node(z).rank.Value).Max();

        }
        /*
 * Returns a new graph with only simple edges. Handles aggregation of data
 * associated with multi-edges.
 */
        public static DagreGraph simplify(DagreGraph g)
        {
            var simplified = new DagreGraph().setGraph(g.graph());
            foreach (var v in g.nodes())
            {
                simplified.setNode(v, g.node(v));
            }
            foreach (var e in g.edges())
            {
                var r = simplified.edge(e.v, e.w);
                var simpleLabel = r == null ? (new DagreLabel() { minlen = 1, weight = 0 }) : r; ;
                var label = g.edge(e);
                simplified.setEdge(e.v, e.w, new DagreLabel
                {
                    weight = simpleLabel.weight + label.weight,
                    minlen = Math.Max(simpleLabel.minlen, label.minlen)
                });
            }


            return simplified;
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
                    layering[rank.Value][node.order.Value] = v;
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

        /*
         * Finds where a line starting at point ({x, y}) would intersect a rectangle
         * ({x, y, width, height}) if it were pointing at the rectangle's center.
         */
        internal static dPoint intersectRect(DagreNode rect, dPoint point)
        {
            var x = rect.x;
            var y = rect.y;

            // Rectangle intersection algorithm from:
            // http://math.stackexchange.com/questions/108113/find-edge-between-two-boxes
            var dx = point.x - x;
            var dy = point.y - y;
            var w = rect.width / 2;
            var h = rect.height / 2;

            if (dx == null && dy == null)
            {
                throw new DagreException("Not possible to find intersection inside of the rectangle");
            }

            double sx, sy;
            if (Math.Abs(dy.Value) * w > Math.Abs(dx.Value) * h)
            {
                // Intersection is top or bottom of rect.
                if (dy < 0)
                {
                    h = -h;
                }
                sx = h.Value * dx.Value / dy.Value;
                sy = h.Value;
            }
            else
            {
                // Intersection is left or right of rect.
                if (dx < 0)
                {
                    w = -w;
                }
                sx = w.Value;
                sy = w.Value * dy.Value / dx.Value;
            }

            return new dPoint { x = x + sx, y = (y + sy) };
        }


    }
}
