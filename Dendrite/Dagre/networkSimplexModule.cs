using System;
using System.Collections.Generic;
using System.Linq;

namespace Dendrite.Dagre
{
    public class networkSimplexModule
    {

        /*
         * The network simplex algorithm assigns ranks to each node in the input graph
         * and iteratively improves the ranking to reduce the length of edges.
         *
         * Preconditions:
         *
         *    1. The input graph must be a DAG.
         *    2. All nodes in the graph must have an object value.
         *    3. All edges in the graph must have "minlen" and "weight" attributes.
         *
         * Postconditions:
         *
         *    1. All nodes in the graph will have an assigned "rank" attribute that has
         *       been optimized by the network simplex algorithm. Ranks start at 0.
         *
         *
         * A rough sketch of the algorithm is as follows:
         *
         *    1. Assign initial ranks to each node. We use the longest path algorithm,
         *       which assigns ranks to the lowest position possible. In general this
         *       leads to very wide bottom ranks and unnecessarily long edges.
         *    2. Construct a feasible tight tree. A tight tree is one such that all
         *       edges in the tree have no slack (difference between length of edge
         *       and minlen for the edge). This by itself greatly improves the assigned
         *       rankings by shorting edges.
         *    3. Iteratively find edges that have negative cut values. Generally a
         *       negative cut value indicates that the edge could be removed and a new
         *       tree edge could be added to produce a more compact graph.
         *
         * Much of the algorithms here are derived from Gansner, et al., "A Technique
         * for Drawing Directed Graphs." The structure of the file roughly follows the
         * structure of the overall algorithm.
         */

        public static void initRank(DagreGraph g)
        {
            longestPath(g);
        }

        public static void initLowLimValues(DagreGraph tree, string root = null)
        {
            if (root == null)
            {
                root = tree.nodes()[0];
            }
            dfsAssignLowLim(tree, new HashSet<string>(), 1, root);
        }

        public static int dfsAssignLowLim(DagreGraph tree, HashSet<string> visited, int nextLim, string v, object parent = null)
        {
            var low = nextLim;
            var label = tree.node(v);

            visited.Add(v);
            foreach (var w in tree.neighbors(v))
            {
                if (!visited.Contains(w))
                {
                    nextLim = dfsAssignLowLim(tree, visited, nextLim, w, v);
                }
            }


            label.low = low;
            label.lim = nextLim++;
            if (parent != null)
            {
                label.parent = parent;
            }
            else
            {
                // TODO should be able to remove this when we incrementally update low lim
                label.parent = null;
                //delete label.parent;
            }

            return nextLim;
        }

        public static string[] postorder(DagreGraph t, string[] g)
        {
            return graphlib.dfs(t, g, "post");
        }
        public static string[] preorder(DagreGraph t, string[] g)
        {
            return graphlib.dfs(t, g, "pre");
        }
        /*
 * Initializes cut values for all edges in the tree.
 */
        public static void initCutValues(DagreGraph t, DagreGraph g)
        {
            var vs = postorder(t, t.nodes());
            //vs = vs.slice(0, vs.length - 1);
            foreach (var v in vs)
            {
                assignCutValue(t, g, v);
            }
        }
        public static void assignCutValue(DagreGraph t, DagreGraph g, string child)
        {
            var childLab = t.node(child);
            var parent = childLab.parent as string;
            t.edge(child, parent).cutvalue = calcCutValue(t, g, child);
        }

        /*
         * Given the tight tree, its graph, and a child in the graph calculate and
         * return the cut value for the edge between the child and its parent.
         */
        public static int calcCutValue(DagreGraph t, DagreGraph g, string child)
        {
            var childLab = t.node(child);
            var parent = childLab.parent as string;
            // True if the child is on the tail end of the edge in the directed graph
            var childIsTail = true;
            // The graph's view of the tree edge we're inspecting
            var graphEdge = g.edge(child, parent);
            // The accumulated cut value for the edge between this node and its parent
            var cutValue = 0;

            if (graphEdge != null)
            {
                childIsTail = false;
                graphEdge = g.edge(parent, child);
            }

            cutValue = graphEdge.weight;

            foreach (var e in g.nodeEdges(child).OfType<DagreEdgeIndex>())
            {
                var isOutEdge = e.v == child;
                var other = isOutEdge ? e.w : e.v;
                if (other != parent)
                {
                    var pointsToHead = isOutEdge == childIsTail;
                    var otherWeight = g.edge(e).weight;

                    cutValue += pointsToHead ? otherWeight : -otherWeight;
                    if (isTreeEdge(t, child, other))
                    {
                        var otherCutValue = t.edge(child, other).cutvalue.Value;
                        cutValue += pointsToHead ? -otherCutValue : otherCutValue;
                    }
                }
            }


            return cutValue;
        }

        /*
         * Returns true if the edge is in the tree.
         */
        public static bool isTreeEdge(DagreGraph tree, string u, string v)
        {
            return tree.hasEdge(u, v);
        }

        public static DagreEdgeIndex enterEdge(DagreGraph t, DagreGraph g, DagreEdgeIndex edge)
        {
            var v = edge.v;
            var w = edge.w;

            // For the rest of this function we assume that v is the tail and w is the
            // head, so if we don't have this edge in the graph we should flip it to
            // match the correct orientation.
            if (!g.hasEdge(v, w))
            {
                v = edge.w;
                w = edge.v;
            }

            var vLabel = t.node(v);
            var wLabel = t.node(w);
            var tailLabel = vLabel;
            var flip = false;

            // If the root is in the tail of the edge then we need to flip the logic that
            // checks for the head and tail nodes in the candidates function below.
            if (vLabel.lim > wLabel.lim)
            {
                tailLabel = wLabel;
                flip = true;
            }

            var candidates = g.edges().Where(ee =>
            {
                return flip == isDescendant(t, t.node(ee.v), tailLabel) &&
                       flip != isDescendant(t, t.node(ee.w), tailLabel);
            });

            //return _.minBy(candidates, function(edge) { return slack(g, edge); });

            return candidates.OrderBy(z => slack(g, z)).First();
        }

        /*
         * Returns true if the specified node is descendant of the root node per the
         * assigned low and lim attributes in the tree.
         */
        public static bool isDescendant(DagreGraph tree, DagreNode vLabel, DagreNode rootLabel)
        {
            return rootLabel.low <= vLabel.lim && vLabel.lim <= rootLabel.lim;
        }

        public static void networkSimplex(DagreGraph g)
        {
            g = util.simplify(g);
            initRank(g);
            var t = feasibleTree(g);

            initLowLimValues(t);
            initCutValues(t, g);

            object e = null, f = null;
            while ((e = leaveEdge(t)) != null)
            {
                f = enterEdge(t, g, e as DagreEdgeIndex);
                exchangeEdges(t, g, e as DagreEdgeIndex, f as DagreEdgeIndex);
            }
        }

        public static void exchangeEdges(DagreGraph t, DagreGraph g, DagreEdgeIndex e, DagreEdgeIndex f)
        {
            var v = e.v;
            var w = e.w;
            t.removeEdge(v, w);
            t.setEdge(f.v, f.w, null);
            initLowLimValues(t);
            initCutValues(t, g);
            updateRanks(t, g);
        }
        public static object leaveEdge(DagreGraph tree)
        {
            return tree.edges().FirstOrDefault(e => tree.edge(e).cutvalue < 0);
        }

        public static void updateRanks(DagreGraph t, DagreGraph g)
        {
            var root = t.nodes().FirstOrDefault(v => { return g.node(v).parent != null; });
            var vs = preorder(t, new string[] { root });
            //vs = vs.slice(1);
            vs = vs.Take(1).ToArray();
            foreach (var v in vs)
            {
                var parent = t.node(v).parent as string;
                var edge = g.edge(v, parent);
                var flipped = false;

                if (edge != null)
                {
                    edge = g.edge(parent, v);
                    flipped = true;
                }

                g.node(v).rank = g.node(parent).rank + (flipped ? edge.minlen : -edge.minlen);
            }

        }

        /*
         * Initializes ranks for the input graph using the longest path algorithm. This
         * algorithm scales well and is fast in practice, it yields rather poor
         * solutions. Nodes are pushed to the lowest layer possible, leaving the bottom
         * ranks wide and leaving edges longer than necessary. However, due to its
         * speed, this algorithm is good for getting an initial ranking that can be fed
         * into other algorithms.
         *
         * This algorithm does not normalize layers because it will be used by other
         * algorithms in most cases. If using this algorithm directly, be sure to
         * run normalize at the end.
         *
         * Pre-conditions:
         *
         *    1. Input graph is a DAG.
         *    2. Input graph node labels can be assigned properties.
         *
         * Post-conditions:
         *
         *    1. Each node will be assign an (unnormalized) "rank" property.
         */
        public static void longestPath(DagreGraph g)
        {
            HashSet<string> visited = new HashSet<string>();

            Func<string, int> dfs = null;
            dfs = (v) =>
            {
                var label = g.node(v);
                if (visited.Contains(v))
                {
                    return label.rank.Value;
                }
                visited.Add(v);
                var rank = g.outEdges(v).Select(e => dfs((e as DagreEdgeIndex).w) - (g.edge(e) as DagreLabel).minlen).Min();

                if (rank == double.MaxValue || // return value of _.map([]) for Lodash 3
                    rank == null // return value of _.map([]) for Lodash 4
                    )
                { // return value of _.map([null])
                    rank = 0;
                }

                label.rank = rank;
                return label.rank.Value;

            };

            foreach (var item in g.sources())
            {
                dfs(item);
            }
        }

        /*
         * Returns the amount of slack for the given edge. The slack is defined as the
         * difference between the length of the edge and its minimum length.
         */
        public static int slack(DagreGraph g, DagreEdgeIndex e)
        {
            return g.node(e.w).rank.Value - g.node(e.v).rank.Value - g.edge(e).minlen;
        }
        public static int slack(DagreGraph g, object e)
        {
            return slack(g, e as DagreEdgeIndex);
        }

        /*
         * Constructs a spanning tree with tight edges and adjusted the input node's
         * ranks to achieve this. A tight edge is one that is has a length that matches
         * its "minlen" attribute.
         *
         * The basic structure for this function is derived from Gansner, et al., "A
         * Technique for Drawing Directed Graphs."
         *
         * Pre-conditions:
         *
         *    1. Graph must be a DAG.
         *    2. Graph must be connected.
         *    3. Graph must have at least one node.
         *    5. Graph nodes must have been previously assigned a "rank" property that
         *       respects the "minlen" property of incident edges.
         *    6. Graph edges must have a "minlen" property.
         *
         * Post-conditions:
         *
         *    - Graph nodes will have their rank adjusted to ensure that all edges are
         *      tight.
         *
         * Returns a tree (undirected graph) that is constructed using only "tight"
         * edges.
         */

        public static DagreGraph feasibleTree(DagreGraph g)
        {
            var t = new DagreGraph() { directed = false };

            // Choose arbitrary node from which to start our tree
            var start = g.nodes()[0];
            var size = g.nodeCount();
            t.setNode(start, null);

            dynamic edge;
            int delta;
            while (tightTree(t, g) < size)
            {
                edge = findMinSlackEdge(t, g);
                delta = t.hasNode(edge.v) ? slack(g, edge) : -slack(g, edge);
                shiftRanks(t, g, delta);
            }

            return t;
        }


        /*
         * Finds a maximal tree of tight edges and returns the number of nodes in the
         * tree.
         */
        public static int tightTree(DagreGraph t, DagreGraph g)
        {
            Action<string> dfs = null;
            dfs = (v) =>
            {
                foreach (var ee in g.nodeEdges(v))
                {
                    var e = ee as DagreEdgeIndex;
                    var edgeV = e.v;
                    var w = (v == edgeV) ? e.w : edgeV;
                    if (!t.hasNode(w) && slack(g, e) != 0)
                    {
                        t.setNode(w, null);
                        t.setEdge(v, w, null);
                        dfs(w);
                    }
                }
            };

            foreach (var item in t.nodes())
            {
                dfs(item);
            }
            return t.nodeCount();
        }

        /*
         * Finds the edge with the smallest slack that is incident on tree and returns
         * it.
         */
        public static DagreEdgeIndex findMinSlackEdge(DagreGraph t, DagreGraph g)
        {
            return g.edges().Where(e =>
            {
                return t.hasNode(e.v) != t.hasNode(e.w);
            }).OrderBy(e => slack(g, e)).First();
        }

        public static void shiftRanks(DagreGraph t, DagreGraph g, int delta)
        {
            foreach (var v in t.nodes())
            {
                g.node(v).rank += delta;
            }

        }
    }

    public class graphlib
    {
        public static string[] dfs(DagreGraph g, string[] vs, string order)
        {

            HashSet<string> visited = new HashSet<string>();



            Func<string, string[]> navigation = null;
            if (g.directed) navigation = (u) =>
               {
                   return g.successors(u);
               };
            else

                navigation = (u) => { return g.neighbors(u); };

            List<string> acc = new List<string>();

            foreach (var v in vs)
            {
                if (!g.hasNode(v))
                {
                    throw new DagreException("graph does not have node: " + v);
                }
                doDfs(g, v, order == "post", visited, navigation, acc);
            }
            return acc.ToArray();
        }

        public static void doDfs(DagreGraph g, string v, bool postorder, HashSet<string> visited, Func<string, string[]> navigation, List<string> acc)
        {
            if (!visited.Contains(v))
            {
                visited.Add(v);
            }
            if (!postorder) { acc.Add(v); }
            foreach (var w in navigation(v))
            {
                doDfs(g, w, postorder, visited, navigation, acc);
            }
            if (postorder)
            {
                acc.Add(v);
            }
        }
    }
}
