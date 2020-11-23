using System;
using System.Collections.Generic;
using System.Linq;

namespace Dendrite.Dagre
{
    public class DagreLayout
    {
        public void layout(DagreGraph g)
        {
            var layoutGraph = buildLayoutGraph(g);
            runLayout(layoutGraph);
            updateInputGraph(g, layoutGraph);
        }

        /*
         * Copies final layout information from the layout graph back to the input
         * graph. This process only copies whitelisted attributes from the layout graph
         * to the input graph, so it serves as a good place to determine what
         * attributes can influence layout.
         */
        private void updateInputGraph(DagreGraph inputGraph, DagreGraph layoutGraph)
        {
            foreach (var v in inputGraph.nodes())
            {
                var inputLabel = inputGraph.node(v);
                var layoutLabel = layoutGraph.node(v);

                if (inputLabel != null)
                {
                    inputLabel.x = layoutLabel.x;
                    inputLabel.y = layoutLabel.y;

                }
                if (layoutGraph.children(v).Length > 0)
                {
                    inputLabel.width = layoutLabel.width;
                    inputLabel.height = layoutLabel.height;
                }
            }

            foreach (var e in inputGraph.edges())
            {
                var inputLabel = inputGraph.edge(e);
                var layoutLabel = layoutGraph.edge(e);
                inputLabel.points = layoutLabel.points;
                if (layoutLabel.x != null)
                {
                    inputLabel.x = layoutLabel.x;
                    inputLabel.y = layoutLabel.y;
                }
            }

            inputGraph.graph().width = layoutGraph.graph().width;
            inputGraph.graph().height = layoutGraph.graph().height;

        }

        /*
         * This idea comes from the Gansner paper: to account for edge labels in our
         * layout we split each rank in half by doubling minlen and halving ranksep.
         * Then we can place labels at these mid-points between nodes.
         *
         * We also add some minimal padding to the width to push the label for the edge
         * away from the edge itself a bit.
         */
        public void makeSpaceForEdgeLabels(DagreGraph g)
        {
            var graph = g.graph();
            graph.ranksep /= 2;
            foreach (var e in g.edges())
            {
                var edge = g.edge(e);
                edge.minlen *= 2;
                if (edge.labelpos.ToLower() != "c")
                {
                    if (graph.rankdir == "TB" || graph.rankdir == "BT")
                    {
                        edge.width += edge.labeloffset;
                    }
                    else
                    {
                        edge.height += edge.labeloffset;
                    }
                }
            }
        }

        public object canonicalize(object attrs)
        {
            if (attrs is DagreLabel dl)
            {
                DagreLabel ret = new DagreLabel();
                ret.nodesep = dl.nodesep;
                ret.ranksep = dl.ranksep;
                return ret;
            }
            if (attrs is DagreEdge de)
            {
                DagreEdge ret = new DagreEdge();
                ret.forwardName = de.forwardName;
                return de;
            }
            if (attrs is DagreNode dn)
            {
                DagreNode ret = new DagreNode();
                ret.width = dn.width;
                ret.height = dn.height;
                return ret;
            }
            /*var newAttrs = { };
            _.forEach(attrs, function(v, k) {
                newAttrs[k.toLowerCase()] = v;
            });
            return newAttrs;*/
            return new object();
        }

        public class GraphDefaults
        {
            public int edgesep = 20;
            public int nodesep = 50;
            public string rankdir = "tb";
            public int ranksep = 50;
        }


        public object merge(object[] list)
        {
            DagreLabel lab = new DagreLabel();
            lab.edgesep = 20;
            lab.nodesep = 25;
            lab.rankdir = "tb";
            lab.ranksep = 20;
            return lab;
            return new object();
        }

        public object pick(object o1, object o2)
        {
            return new object();
        }
        public object selectNumberAttrs(object label, string[] attrs)
        {
            //clone with selected args??
            return label;
        }

        public object defaults(object o1, object o2)
        {
            if(o1 is DagreNode dn)
            {
                if (o2  is NodeDefaults nd)
                {
                    if (dn.width == null)
                    {
                        dn.width = nd.width;
                    }
                    if (dn.height == null)
                    {
                        dn.height = nd.height;
                    }
                    return dn;
                }
                else
                {
                    throw new NotImplementedException();
                }

            }
            throw new NotImplementedException();
        }

        public class EdgeDefaults
        {
            public int height = 0;
            public int labeloffset = 10;
            public string labelpos = "r";
            public int minlen = 1;
            public int weight = 1;
            public int width = 0;
        }
        public class NodeDefaults
        {
            public int height = 0;
            public int width = 0;
        }
        /*
         * Constructs a new graph from the input graph, which can be used for layout.
         * This process copies only whitelisted attributes from the input graph to the
         * layout graph. Thus this function serves as a good place to determine what
         * attributes can influence layout.
         */
        public DagreGraph buildLayoutGraph(DagreGraph inputGraph)
        {
            string[] graphNumAttrs = new string[] { "nodesep", "edgesep", "ranksep", "marginx", "narginy" };
            string[] edgeNumAttrs = new string[] { "minlen", "weight", "width", "height", "labeloffset" };
            string[] nodeNumAttrs = new string[] { "width", "height" };
            string[] graphAttrs = new string[] { "acyclicer", "ranker", "rankdir", "align" };
            string[] edgeAttrs = new string[] { "labelpos" };
            var g = new DagreGraph() { multigraph = true, compound = true };
            var graph = canonicalize(inputGraph.graph());

            g.setGraph(merge(new object[] { null, new GraphDefaults(), selectNumberAttrs(graph, graphNumAttrs), pick(graph, graphAttrs) }));


            foreach (var v in inputGraph.nodes())
            {
                var node = canonicalize(inputGraph.node(v));
                g.setNode(v, defaults(selectNumberAttrs(node, nodeNumAttrs), new NodeDefaults()));
                g.setParent(v, inputGraph.parent(v));


            }


            foreach (var e in inputGraph.edges())
            {
                var edge = canonicalize(inputGraph.edge(e));
                g.setEdge(e, merge(new object[] { null, new EdgeDefaults(), selectNumberAttrs(edge, edgeNumAttrs), pick(edge, edgeAttrs) }));
            
            }


            return g;
        }

        public void removeSelfEdges(DagreGraph g)
        {
            var ar = g.edges().ToArray();
            foreach (var e in ar)
            {
                if (e.v == e.w)
                {
                    var node = g.node(e.v);
                    if (node.selfEdges == null)
                    {
                        node.selfEdges = new List<SelfEdgeInfo>();
                    }
                    node.selfEdges.Add(new SelfEdgeInfo() { e = e, label = g.edge(e) });
                    g.removeEdge(e);
                }
            }

        }

        public void rank(DagreGraph g)
        {
            switch (g.graph().ranker)
            {
                case "network-simplex":
                    throw new NotImplementedException();
                    break;
                case "tight-tree":
                    throw new NotImplementedException();
                    break;
                case "longest-path":
                    throw new NotImplementedException();
                    break;
                default:
                    networkSimplexRanker(g);
                    break;
            }
        }
        public void networkSimplexRanker(DagreGraph g)
        {
            networkSimplexModule.networkSimplex(g);
        }

        public void runLayout(DagreGraph g)
        {
            makeSpaceForEdgeLabels(g);
            removeSelfEdges(g);
            acyclic.run(g);

            nestingGraph.run(g);
            
            rank(util.asNonCompoundGraph(g));
            /*
            injectEdgeLabelProxies(g);
            removeEmptyRanks(g);
            nestingGraph.cleanup(g);*/

            util.normalizeRanks(g);

            assignRankMinMax(g);

            removeEdgeLabelProxies(g);

            normalize.run(g);

            parentDummyChains._parentDummyChains(g);

            addBorderSegments._addBorderSegments(g);
            /*
            order(g);*/
            insertSelfEdges(g);
            /*
            coordinateSystem.adjust(g);
            position(g);
            positionSelfEdges(g);*/
            removeBorderNodes(g);

            normalize.undo(g);
            /*
            fixupEdgeLabelCoords(g);
            coordinateSystem.undo(g);
            translateGraph(g);
            assignNodeIntersects(g);
            reversePointsForReversedEdges(g);
            acyclic.undo(g);*/
        }
        public static void removeBorderNodes(DagreGraph g)
        {
            foreach (var v in g.nodes())
            {
                if (g.children(v).Length > 0)
                {
                    var node = g.node(v);
                    var t = g.node(node.borderTop);
                    //var b = g.node(node.borderLeft);
                    throw new NotImplementedException();
                    //var l = g.node(_.last(node.borderLeft));
                    //var r = g.node(_.last(node.borderRight));
                    /*
                    node.width = Math.Abs(r.x - l.x);
                    node.height = Math.Abs(b.y - t.y);
                    node.x = l.x + node.width / 2;
                    node.y = t.y + node.height / 2;*/
                }
            }

            foreach (var v in g.nodes())
            {
                if (g.node(v).dummy == "border")
                {
                    g.removeNode(v);
                }
            }


        }

        private void insertSelfEdges(DagreGraph g)
        {
            var layers = util.buildLayerMatrix(g);
            foreach (var layer in layers)
            {
                var orderShift = 0;
                for (int i = 0; i < layer.Length; i++)
                {
                    var v = layer[i];
                    var node = g.node(v);
                    throw new NotImplementedException();
                    //node.order = i + orderShift;
                    foreach (var selfEdge in node.selfEdges)
                    {

                    }
                }
            }
        }

        private void removeEdgeLabelProxies(DagreGraph g)
        {
            foreach (var v in g.nodes())
            {
                var node = g.node(v);
                if (node.dummy == "edge-proxy")
                {
                    g.edge(node.e).labelRank = node.rank;
                    g.removeNode(v);
                }
            }

        }

        private void assignRankMinMax(DagreGraph g)
        {
            int maxRank = 0;
            foreach (var v in g.nodes())
            {
                var node = g.node(v);
                if (node.borderTop != null)
                {
                    node.minRank = g.node(node.borderTop).rank;
                    //node.maxRank = g.node(node.borderLeft).rank;
                    maxRank = Math.Max(maxRank, node.maxRank.Value);
                }
            }

            g.graph().maxRank = maxRank;
        }
    }
}
