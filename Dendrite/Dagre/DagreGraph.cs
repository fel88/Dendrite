using System;
using System.Collections.Generic;
using System.Linq;

namespace Dendrite.Dagre
{
    public class DagreGraph
    {
        public bool multigraph;
        public bool compound;
        public List<DagreNode> nodes = new List<DagreNode>();
        public List<DagreEdge> _edges = new List<DagreEdge>();
        DagreLabel _label = new DagreLabel();
        public DagreLabel graph() { return _label; }

        public DagreNode node(string v) { return nodes.First(z => z.key == v);/*replace with dictionary*/ }
        public DagreNode node(DagreNode v) { return node(v.key); }
        List<DagreEdgeIndex> _edgesIndexes = new List<DagreEdgeIndex>();

        public DagreEdge edge(string v)
        {
            /*replace with dictioanry*/
            return _edges.First(z => z.key == v);
        }
        public DagreEdge edge(DagreEdgeIndex v)
        {
            throw new NotImplementedException();
            //return _edges[v];
        }
        public List<DagreEdgeIndex> edges()
        {
            return _edgesIndexes;
        }

        internal void removeEdge(DagreEdgeIndex e)
        {
            _edgesIndexes.Remove(e);
        }

        Dictionary<string, DagreEdgeIndex[]> _out = new Dictionary<string, DagreEdgeIndex[]>();

        internal void setEdge(string edgeObj, DagreEdge origLabel)
        {
            throw new NotImplementedException();
        }

        internal bool hasNode(DagreNode v)
        {
            return nodes.Any(z => z.key == v.key);

        }

        internal DagreNode[] successors(DagreNode v)
        {
            throw new NotImplementedException();
        }

        internal DagreNode[] children(DagreNode v = null)
        {
            if (v == null)
            {

            }
            throw new NotImplementedException();
        }

        internal DagreEdgeIndex[] outEdges(string v)
        {
            if (_out.ContainsKey(v))
            {
                var outV = _out[v];
                if (outV != null && outV.Any())
                {
                    return outV;
                }
                return _edgesIndexes.Where(z => z.v == z.w).ToArray();
            }
            return null;


        }

        internal void setEdge(string w, string v, DagreEdge label, Guid guid)
        {
            throw new NotImplementedException();
        }


        internal object edge(object e)
        {
            throw new NotImplementedException();
        }

        internal void removeNode(DagreNode v)
        {
            throw new NotImplementedException();
        }
    }
    public class DagreNode
    {
        public string key;
        public List<SelfEdgeInfo> selfEdges = null;
        public int? rank;
        internal double x;
        internal double y;
        internal double width;
        internal double height;
        internal string borderTop;
        internal int? minRank;
        internal int? maxRank;
        internal string borderBottom;
        internal string dummy;
        internal string e;
        internal string order;
        internal DagreEdge edgeLabel;
        internal string edgeObj;
    }
    public class SelfEdgeInfo
    {
        public DagreEdge label;
        public DagreEdgeIndex e;
    }
    public class DagreEdge
    {
        public string forwardName;
        public string key;
        public double minlen;
        public int weight;
        public double width;
        public double height;
        public int labeloffset;
        public string labelpos;
        internal bool reversed;
        internal double? x;
        internal double? y;
        internal List<dPoint> points = new List<dPoint>();
        internal int? labelRank;
    }

    public class dPoint
    {
        public double x;
        public double y;
    }
    public class DagreEdgeIndex
    {
        public string v;
        public string w;
        public string name;
    }

    public class DagreLabel
    {
        public int edgesep;
        public int nodesep;
        public string rankdir;
        public int ranksep;
        public string acyclicer;
        internal double width;
        internal double height;
        internal int maxRank;
        internal DagreNode nestingRoot;
        internal int nodeRankFactor;
        internal List<DagreNode> dummyChains;
    }

}
