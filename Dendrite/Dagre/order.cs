using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dendrite.Dagre
{
    public class order
    {

        /*
         * Applies heuristics to minimize edge crossings in the graph and sets the best
         * order solution as an order attribute on each node.
         *
         * Pre-conditions:
         *
         *    1. Graph must be DAG
         *    2. Graph nodes must be objects with a "rank" attribute
         *    3. Graph edges must have the "weight" attribute
         *
         * Post-conditions:
         *
         *    1. Graph nodes will have an "order" attribute based on the results of the
         *       algorithm.
         */
        public static DagreGraph[] buildLayerGraphs(DagreGraph g, int[] ranks, string relationship)
        {
            return ranks.Select(z => buildLayerGraph(g, z, relationship)).ToArray();
        }


        public static void _order(DagreGraph g)
        {

            var maxRank = util.maxRank(g);
            var downLayerGraphs = buildLayerGraphs(g, util.range(1, maxRank + 1), "inEdges");
            var upLayerGraphs = buildLayerGraphs(g, util.range(maxRank - 1, -1, -1), "outEdges");


            var layering = initOrder(g);
            assignOrder(g, layering);

            int bestCC = int.MaxValue;
            string[][] best = null;

            for (int i = 0, lastBest = 0; lastBest < 4; ++i, ++lastBest)
            {
                sweepLayerGraphs((i % 2 == 0) ? downLayerGraphs : upLayerGraphs, i % 4 >= 2);

                layering = util.buildLayerMatrix(g);
                var cc = crossCount(g, layering);
                if (cc < bestCC)
                {
                    lastBest = 0;
                    best = util.cloneDeep(layering);
                    bestCC = cc;
                }
            }

            assignOrder(g, best);

        }

        public static void sweepLayerGraphs(DagreGraph[] layerGraphs, bool biasRight)
        {
            var cg = new DagreGraph();
            foreach (var lg in layerGraphs)
            {
                var root = lg.graph().root;
                var sorted = sortSubGraphModule.sortSubraph(lg, root, cg, biasRight);
                //foreach (var item in sorted)
                {

                }
            }
            /*
            _.forEach(layerGraphs, function(lg) {
                var root = lg.graph().root;
                var sorted = sortSubgraph(lg, root, cg, biasRight);
                _.forEach(sorted.vs, function(v, i) {
                    lg.node(v).order = i;
                });
                addSubgraphConstraints(lg, cg, sorted.vs);
            });*/
        }

        public static void assignOrder(DagreGraph g, string[][] layering)
        {
            foreach (var layer in layering)
            {

                for (int i = 0; i < layer.Length; i++)
                {
                    var v = layer[i];
                    g.node(v).order = i;
                }


            }
            //_.forEach(layering, function(layer) {
            //    _.forEach(layer, function(v, i) {
            //        g.node(v).order = i;
            //    });
            //});
        }

        #region buildLayerGraph

        /*
         * Constructs a graph that can be used to sort a layer of nodes. The graph will
         * contain all base and subgraph nodes from the request layer in their original
         * hierarchy and any edges that are incident on these nodes and are of the type
         * requested by the "relationship" parameter.
         *
         * Nodes from the requested rank that do not have parents are assigned a root
         * node in the output graph, which is set in the root graph attribute. This
         * makes it easy to walk the hierarchy of movable nodes during ordering.
         *
         * Pre-conditions:
         *
         *    1. Input graph is a DAG
         *    2. Base nodes in the input graph have a rank attribute
         *    3. Subgraph nodes in the input graph has minRank and maxRank attributes
         *    4. Edges have an assigned weight
         *
         * Post-conditions:
         *
         *    1. Output graph has all nodes in the movable rank with preserved
         *       hierarchy.
         *    2. Root nodes in the movable layer are made children of the node
         *       indicated by the root attribute of the graph.
         *    3. Non-movable nodes incident on movable nodes, selected by the
         *       relationship parameter, are included in the graph (without hierarchy).
         *    4. Edges incident on movable nodes, selected by the relationship
         *       parameter, are added to the output graph.
         *    5. The weights for copied edges are aggregated as need, since the output
         *       graph is not a multi-graph.
         */
        public static DagreGraph buildLayerGraph(DagreGraph g, int rank, string relationship)
        {
            var result = new DagreGraph() { compound = true };
            /*var root = createRootNode(g),
              result = new Graph({ compound: true }).setGraph({ root: root })
      .setDefaultNodeLabel(function(v) { return g.node(v); });

  _.forEach(g.nodes(), function(v)
        {
            var node = g.node(v),
              parent = g.parent(v);

            if (node.rank === rank || node.minRank <= rank && rank <= node.maxRank)
            {
                result.setNode(v);
                result.setParent(v, parent || root);

                // This assumes we have only short edges!
                _.forEach(g[relationship](v), function(e) {
                    var u = e.v === v ? e.w : e.v,
                      edge = result.edge(u, v),
                      weight = !_.isUndefined(edge) ? edge.weight : 0;
                    result.setEdge(u, v, { weight: g.edge(e).weight + weight });
                });

                if (_.has(node, "minRank"))
                {
                    result.setNode(v, {
                    borderLeft: node.borderLeft[rank],
          borderRight: node.borderRight[rank]
                    });
                }
            }
        });

  return result;*/
            return result;
        }

        public static void createRootNode(DagreGraph g)
        {
            //util.uniqueId("_root")
            /*   var v;
               while (g.hasNode((v = _.uniqueId("_root")))) ;
               return v;*/
        }
        #endregion

        #region init order
        /*
 * Assigns an initial order value for each node by performing a DFS search
 * starting from nodes in the first rank. Nodes are assigned an order in their
 * rank as they are first visited.
 *
 * This approach comes from Gansner, et al., "A Technique for Drawing Directed
 * Graphs."
 *
 * Returns a layering matrix with an array per layer and each layer sorted by
 * the order of its nodes.
 */
        public static string[][] initOrder(DagreGraph g)
        {
            List<List<string>> ret = new List<List<string>>();
            /*   var visited = { };
               var simpleNodes = _.filter(g.nodes(), function(v) {
                   return !g.children(v).length;
               });
               var maxRank = _.max(_.map(simpleNodes, function(v) { return g.node(v).rank; }));
               var layers = _.map(_.range(maxRank + 1), function() { return []; });

               function dfs(v)
               {
                   if (_.has(visited, v)) return;
                   visited[v] = true;
                   var node = g.node(v);
                   layers[node.rank].push(v);
                   _.forEach(g.successors(v), dfs);
               }

               var orderedVs = _.sortBy(simpleNodes, function(v) { return g.node(v).rank; });
               _.forEach(orderedVs, dfs);

               return layers;*/
            return ret.Select(z => z.ToArray()).ToArray();
        }

        #endregion

        #region cross-count

        /*
         * A function that takes a layering (an array of layers, each with an array of
         * ordererd nodes) and a graph and returns a weighted crossing count.
         *
         * Pre-conditions:
         *
         *    1. Input graph must be simple (not a multigraph), directed, and include
         *       only simple edges.
         *    2. Edges in the input graph must have assigned weights.
         *
         * Post-conditions:
         *
         *    1. The graph and layering matrix are left unchanged.
         *
         * This algorithm is derived from Barth, et al., "Bilayer Cross Counting."
         */
        public static int crossCount(DagreGraph g, string[][] layering)
        {
            var cc = 0;
            for (var i = 1; i < layering.Length; ++i)
            {
                cc += twoLayerCrossCount(g, layering[i - 1], layering[i]);
            }
            return cc;
        }

        public static int twoLayerCrossCount(DagreGraph g, string[] northLayer, string[] southLayer)
        {
            // Sort all of the edges between the north and south layers by their position
            // in the north layer and then the south. Map these edges to the position of
            // their head in the south layer.
            /*    var southPos = _.zipObject(southLayer,
                  _.map(southLayer, function(v, i) { return i; }));
                var southEntries = _.flatten(_.map(northLayer, function(v) {
                    return _.sortBy(_.map(g.outEdges(v), function(e) {
                        return { pos: southPos[e.w], weight: g.edge(e).weight };
                    }), "pos");
                }), true);*/

            // Build the accumulator tree
            var firstIndex = 1;
            while (firstIndex < southLayer.Length) firstIndex <<= 1;
            var treeSize = 2 * firstIndex - 1;
            firstIndex -= 1;
            var tree = Enumerable.Range(0, treeSize).Select(z => 0).ToArray();
            //var tree = _.map(new Array(treeSize), function() { return 0; });

            // Calculate the weighted crossings
            var cc = 0;
            /*
            _.forEach(southEntries.forEach(function(entry) {
                var index = entry.pos + firstIndex;
                tree[index] += entry.weight;
                var weightSum = 0;
                while (index > 0)
                {
                    if (index % 2)
                    {
                        weightSum += tree[index + 1];
                    }
                    index = (index - 1) >> 1;
                    tree[index] += entry.weight;
                }
                cc += entry.weight * weightSum;
            }));
            */
            return cc;
        }
        #endregion
    }
    public class barycenterDto
    {

        public string v;
        public int? barycenter = null;
        public int weight;
    }
}
