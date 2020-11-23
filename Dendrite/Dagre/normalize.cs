namespace Dendrite.Dagre
{
    public class normalize
    {

        /*
         * Breaks any long edges in the graph into short segments that span 1 layer
         * each. This operation is undoable with the denormalize function.
         *
         * Pre-conditions:
         *
         *    1. The input graph is a DAG.
         *    2. Each node in the graph has a "rank" property.
         *
         * Post-condition:
         *
         *    1. All edges in the graph have a length of 1.
         *    2. Dummy nodes are added where edges have been split into segments.
         *    3. The graph is augmented with a "dummyChains" attribute which contains
         *       the first dummy in each chain of dummy nodes produced.
         */
        public static void run(DagreGraph g)
        {
            g.graph().dummyChains = new System.Collections.Generic.List<DagreNode>();
            foreach (var edge in g.edges())
            {
                normalizeEdge(g, edge);
            }

        }

        public static void undo(DagreGraph g)
        {
            foreach (var vv in g.graph().dummyChains)
            {
                var v = vv;
                var node = g.node(v);
                var origLabel = node.edgeLabel;
                DagreNode w = null;
                g.setEdge(node.edgeObj, origLabel);
                while (node.dummy != null)
                {
                    w = g.successors(v)[0];
                    g.removeNode(v);
                    origLabel.points.Add(new dPoint() { x = node.x.Value, y = node.y.Value });
                    if (node.dummy == "edge-label")
                    {
                        origLabel.x = node.x;
                        origLabel.y = node.y;
                        origLabel.width = node.width.Value;
                        origLabel.height = node.height.Value;
                    }
                    v = w;
                    node = g.node(v);
                }
            }

        }

        public static void normalizeEdge(DagreGraph g, DagreEdgeIndex edge)
        {

        }
    }

}
