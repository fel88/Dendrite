using System;
using System.Collections.Generic;
using System.Linq;

namespace Dendrite.Dagre
{
    public class nestingGraph
    {

        /*
         * A nesting graph creates dummy nodes for the tops and bottoms of subgraphs,
         * adds appropriate edges to ensure that all cluster nodes are placed between
         * these boundries, and ensures that the graph is connected.
         *
         * In addition we ensure, through the use of the minlen property, that nodes
         * and subgraph border nodes to not end up on the same rank.
         *
         * Preconditions:
         *
         *    1. Input graph is a DAG
         *    2. Nodes in the input graph has a minlen attribute
         *
         * Postconditions:
         *
         *    1. Input graph is connected.
         *    2. Dummy nodes are added for the tops and bottoms of subgraphs.
         *    3. The minlen attribute for nodes is adjusted to ensure nodes do not
         *       get placed on the same rank as subgraph border nodes.
         *
         * The nesting graph idea comes from Sander, "Layout of Compound Directed
         * Graphs."
         */
        public static void run(DagreGraph g)
        {
            var root = util.addDummyNode(g, "root", null, "_root");
            var depths = treeDepths(g);
            Dictionary<string, int> d = new Dictionary<string, int>();

            var height = depths.Max(z => z.Value) - 1;// Note: depths is an Object not an array
            var nodeSep = 2 * height + 1;

            g.graph().nestingRoot = root;


            // Multiply minlen by nodeSep to align nodes on non-border ranks.
            foreach (var e in g.edges())
            {
                g.edge(e).minlen *= nodeSep;

            }

            // Calculate a weight that is sufficient to keep subgraphs vertically compact
            var weight = sumWeights(g) + 1;

            // Create border nodes and link them up
            foreach (var child in g.children())
            {
                dfs(g, root, nodeSep, weight, height, depths, child);

            }


            // Save the multiplier for node layers for later removal of empty border
            // layers.
            g.graph().nodeRankFactor = nodeSep;
        }

        public static void dfs(DagreGraph g, DagreNode root, int nodeSep, int weight, int height, Dictionary<string, int> depths, DagreNode v)
        {
            var children = g.children(v);
            if (children == null || children.Length == 0)
            {
                if (v != root)
                {
                    //g.setEdge(root, v, { weight: 0, minlen: nodeSep });
                }
                return;
            }


            var top = util.addBorderNode(g, "_bt");
            var bottom = util.addBorderNode(g, "_bb");
            var label = g.node(v.key);
        }

        public static int sumWeights(DagreGraph g)
        {
            return g.edges().Sum(z => g.edge(z).weight);

        }

        public static Dictionary<string, int> treeDepths(DagreGraph g)
        {
            Dictionary<string, int> depths = new Dictionary<string, int>();
            Action<DagreNode, int> dfs = null;
            dfs = (v, depth) =>
            {
                var children = g.children(v);
                if (children != null && children.Length > 0)
                {
                    foreach (var child in children)
                    {
                        dfs(child, depth + 1);
                    }
                }
                depths.Add(v.key, depth);
            };

            foreach (var v in g.children())
            {
                dfs(v, 1);
            }
            return depths;
        }
    }

}
