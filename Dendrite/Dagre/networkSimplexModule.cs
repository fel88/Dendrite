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

        public static void networkSimplex(DagreGraph g)
        {
            g = util.simplify(g);
            initRank(g);
            /*var t = feasibleTree(g);
            initLowLimValues(t);
            initCutValues(t, g);

            var e, f;
            while ((e = leaveEdge(t)))
            {
                f = enterEdge(t, g, e);
                exchangeEdges(t, g, e, f);
            }*/
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
                var rank = g.outEdges(v).Select(e => dfs(e.w) - g.edge(e).minlen).Min();

                if (rank == double.MaxValue || // return value of _.map([]) for Lodash 3
                    rank == null // return value of _.map([]) for Lodash 4
                    )
                { // return value of _.map([null])
                    rank = 0;
                }

                label.rank = rank;
                return label.rank.Value;

            };
            /*var visited = { };

            function dfs(v)
            {
                var label = g.node(v);
                if (_.has(visited, v))
                {
                    return label.rank;
                }
                visited[v] = true;

                var rank = _.min(_.map(g.outEdges(v), function(e) {
                    return dfs(e.w) - g.edge(e).minlen;
                }));

            if (rank === Number.POSITIVE_INFINITY || // return value of _.map([]) for Lodash 3
                rank === undefined || // return value of _.map([]) for Lodash 4
                rank === null)
            { // return value of _.map([null])
                rank = 0;
            }

            return (label.rank = rank);
        }

        _.forEach(g.sources(), dfs);*/
            foreach (var item in g.sources())
            {
                dfs(item);
            }
        }

        /*
         * Returns the amount of slack for the given edge. The slack is defined as the
         * difference between the length of the edge and its minimum length.
         */
        public double slack(DagreGraph g, DagreEdgeIndex e)
        {
            return g.node(e.w).rank.Value - g.node(e.v).rank.Value - g.edge(e).minlen;
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

        public void feasibleTree(DagreGraph g)
        {
            var t = new DagreGraph() { directed = false };

            // Choose arbitrary node from which to start our tree
            var start = g.nodes()[0];
            var size = g.nodeCount();
            t.setNode(start, null);

          /*  var edge, delta;
            while (tightTree(t, g) < size)
            {
                edge = findMinSlackEdge(t, g);
                delta = t.hasNode(edge.v) ? slack(g, edge) : -slack(g, edge);
                shiftRanks(t, g, delta);
            }
          
            return t;*/
        }


        /*
         * Finds a maximal tree of tight edges and returns the number of nodes in the
         * tree.
         */
        public void tightTree(object t, DagreGraph g)
        {
            //            function dfs(v)
            //            {
            //                _.forEach(g.nodeEdges(v), function(e) {
            //                    var edgeV = e.v,
            //                      w = (v === edgeV) ? e.w : edgeV;
            //                    if (!t.hasNode(w) && !slack(g, e))
            //                    {
            //                        t.setNode(w, { });
            //            t.setEdge(v, w, { });
            //            dfs(w);
            //        }
            //    });
            //  }

            //_.forEach(t.nodes(), dfs);
            //  return t.nodeCount();
        }

        /*
         * Finds the edge with the smallest slack that is incident on tree and returns
         * it.
         */
        public double findMinSlackEdge(DagreGraph t, DagreGraph g)
        {
            return g.edges().Where(e =>
            {
                return t.hasNode(e.v) != t.hasNode(e.w);
            }).Select(e => slack(g, e)).Min();            
        }

        public void shiftRanks(DagreGraph t, DagreGraph g, int delta)
        {
            foreach (var v in t.nodes())
            {
                g.node(v).rank += delta;
            }

        }
    }
}
