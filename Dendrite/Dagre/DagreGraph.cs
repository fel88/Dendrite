using System;
using System.Collections.Generic;
using System.Linq;

namespace Dendrite.Dagre
{
    public class DagreGraph
    {
        public DagreGraph()
        {
            _children.Add(GRAPH_NODE as string, new Dictionary<string, object>());
        }
        public bool multigraph;

        public bool compound;
        public bool directed = true;

        public string[] nodes()
        {
            return _nodes2.Keys.ToArray();
        }
        public Dictionary<string, DagreNode> _nodes2 = new Dictionary<string, DagreNode>();
        public List<DagreNode> _nodes = new List<DagreNode>();
        public List<DagreEdge> _edges = new List<DagreEdge>();
        DagreLabel _label = new DagreLabel();
        public DagreLabel graph() { return _label; }

        public DagreNode node(string v) { return _nodes2[v]; }

        internal bool isMultigraph()
        {
            return multigraph;
        }

        public DagreNode node(DagreNode v) { return node(v.key); }
        public List<DagreEdgeIndex> _edgesIndexes = new List<DagreEdgeIndex>();

        public DagreEdge edge(string v)
        {
            /*replace with dictioanry*/
            return _edges.First(z => z.key == v);
        }

        public static string edgeArgsToId(bool isDirectred, string v, string w, string name)
        {
            if (!isDirectred && int.Parse(v) > int.Parse(w))
            {
                var tmp = v;
                v = w;
                w = tmp;

            }
            return v + EDGE_KEY_DELIM + w + EDGE_KEY_DELIM + (name == null ? DEFAULT_EDGE_NAME : name);
        }

        public DagreEdgeIndex edgeArgsToObj(bool isDirectred, string v, string w, string name)
        {

            if (!isDirectred && int.Parse(v) > int.Parse(w))
            {
                var tmp = v;
                v = w;
                w = tmp;

            }
            var ret = new DagreEdgeIndex()
            {
                v = v,
                w = w,
                name = name
            };
            return ret;
        }
        public static string EDGE_KEY_DELIM = "☻";
        public static string DEFAULT_EDGE_NAME = " ";

        public object edgeObjToId(bool isDirectred, string v, string w, string name)
        {
            return new object();
        }
        public DagreLabel edge(DagreEdgeIndex v)
        {
            return edge(v.v, v.w, v.name);

        }
        public DagreLabel edge(string v, string w, string name = null)
        {
            var e =
            edgeArgsToId(directed, v, w, name);
            if (_edgeLabels.ContainsKey(e as string)) return null;
            return _edgeLabels[e as string] as DagreLabel;
            //return _edges[v];
        }
        public Dictionary<string, object> _edgeLabels = new Dictionary<string, object>();
        public DagreEdgeIndex[] edges()
        {
            return _edgeObjs.Values.Select(z => z as DagreEdgeIndex).ToArray();
        }

        internal void removeEdge(DagreEdgeIndex e)
        {
            _edgesIndexes.Remove(e);
        }


        internal DagreGraph setEdge(string v, string w, object obj)
        {
            return setEdge(new DagreEdgeIndex() { v = v, w = w }, obj);

        }
        internal DagreGraph setEdge(object edge, object origLabel)
        {
            var ee = edge as DagreEdgeIndex;

            bool valueSpecified = false;
            object val = null;
            if (origLabel is DagreLabel)
            {
                val = origLabel;
                valueSpecified = true;
            }

            var v = ee.v;
            var w = ee.w;
            var name = ee.name;
            /*if (ee.name != null)
            {
                name = ""+ee.name;
            }*/

            var e = edgeArgsToId(directed, v, w, name);
            if (_edgeLabels.ContainsKey(e))
            {
                if (valueSpecified)
                {
                    _edgeLabels[e] = val;

                }
                return this;
            }

            if (ee.name == null && !multigraph)
            {
                throw new DagreException("cannot set a named edge when isMultigraph = false");
            }


            setNode(ee.v);
            setNode(ee.w);
            addOrUpdate(e, _edgeLabels, valueSpecified ? val : _defaultEdgeLabelFn(ee.v, ee.w, ee.name));
            //_edgeLabels[e] = valueSpecified ? val : _defaultEdgeLabelFn(ee.v, ee.w, ee.name);

            var edgeObj = edgeArgsToObj(directed, ee.v, ee.w, ee.name);
            addOrUpdate(e, _edgeObjs, edgeObj);

            incrementOrInitEntry(_preds[w], v);
            incrementOrInitEntry(_sucs[v], w);
            _in[w][e] = edgeObj;
            _out[v][e] = edgeObj;
            _edgesCount++;
            return this;
        }

        internal string[] sources()
        {
            return nodes().Where(v =>
            {
                if (!_in.ContainsKey(v))
                {
                    return false;
                }
                return _in[v].Count == 0;
            }).ToArray();
        }

        private void incrementOrInitEntry(Dictionary<string, object> dictionaries, object v)
        {
            if (dictionaries.ContainsKey(v as string))
            {
                var vv = (int)(dictionaries[v as string]);
                dictionaries[v as string] = vv + 1;
            }
            else
            {
                dictionaries.Add(v as string, 1);
            }
        }

        public Dictionary<string, object> _edgeObjs = new Dictionary<string, object>();

        private object _defaultEdgeLabelFn(string v, string w, string name)
        {
            throw new NotImplementedException();
        }

        int _edgesCount = 0;

        internal bool hasNode(string v)
        {
            return _nodes2.ContainsKey(v);

        }

        internal DagreNode[] successors(DagreNode v)
        {
            throw new NotImplementedException();
        }
        internal string[] children(object v = null)
        {
            if (v == null)
            {
                v = GRAPH_NODE;

            }
            if (compound)
            {
                if (_children.ContainsKey(v as string))
                {
                    var children = _children[v as string];
                    return children.Keys.ToArray();
                }

            }
            else if (v == GRAPH_NODE)
            {
                return nodes();
            }
            else if (hasNode(v as string))
            {
                return new string[] { };
            }
            return null;
        }


        internal DagreEdgeIndex[] outEdges(string v)
        {
            if (_out.ContainsKey(v))
            {
                var outV = _out[v];
                if (outV != null && outV.Any())
                {
                    return outV.Select(z => z.Value as DagreEdgeIndex).ToArray();
                }
                return _edgesIndexes.Where(z => z.v == z.w).ToArray();
            }
            return null;


        }

        internal int nodeCount()
        {
            return _nodes2.Count;
        }

        internal void setEdge(string w, string v, object label, Guid guid)
        {
            throw new NotImplementedException();
        }


        internal object edge(object e)
        {
            throw new NotImplementedException();
        }
        internal void removeNode(string v)
        {
            throw new NotImplementedException();
        }
        internal void removeNode(DagreNode v)
        {
            throw new NotImplementedException();
        }

        internal object inEdges(string v)
        {
            throw new NotImplementedException();
        }

        object _defaultNodeLabelFn(object v)
        {
            return v;
        }
        public DagreGraph setNode(object v, object o2 = null)
        {
            if (_nodes2.ContainsKey(v as string))
            {
                if (o2 != null)
                {
                    _nodes2[v as string] = o2 as DagreNode;

                }
                return this;
            }
            _nodes2.Add(v as string, o2 as DagreNode);


            _nodes2[v as string] = (o2 != null ? o2 : _defaultNodeLabelFn(v)) as DagreNode;
            if (compound)
            {
                addOrUpdate(v as string, _parent, GRAPH_NODE);

                if (!_children.ContainsKey(v as string))
                {
                    _children.Add(v as string, null);
                }
                _children[v as string] = new Dictionary<string, object>();
                if (!_children.ContainsKey(GRAPH_NODE as string))
                {
                    _children.Add(GRAPH_NODE as string, null);
                }
                addOrUpdate(v as string, _children[GRAPH_NODE as string], true);

            }

            addOrUpdate(v as string, _in, new Dictionary<string, object>());
            addOrUpdate(v as string, _preds, new Dictionary<string, object>());
            addOrUpdate(v as string, _out, new Dictionary<string, object>());
            addOrUpdate(v as string, _sucs, new Dictionary<string, object>());



            _nodeCount++;
            return this;
        }
        public void addOrUpdate(string key, Dictionary<string, Dictionary<string, object>> dic, object obj)
        {
            if (!dic.ContainsKey(key))
            {
                dic.Add(key, null);
            }
            dic[key] = obj as Dictionary<string, object>;
        }
        public void addOrUpdate(string key, Dictionary<string, object> dic, object obj)
        {
            if (!dic.ContainsKey(key))
            {
                dic.Add(key, null);
            }
            dic[key] = obj;
        }

        int _nodeCount;

        internal DagreGraph setGraph(object v)
        {
            _label = v as DagreLabel;
            return this;
        }

        internal void setParent(string v, object parent)
        {
            if (!compound)
            {
                throw new DagreException("cannot set parent in non-compound graph");
            }
            if (parent == null)
            {
                parent = GRAPH_NODE;
            }
            else
            {
                throw new NotImplementedException();
            }
            setNode(v);
            removeFromParentsChildList(v);
            _parent[v] = parent;
            _children[parent as string][v] = true;
        }

        void removeFromParentsChildList(string v)
        {
            _children[_parent[v as string] as string].Remove(v as string);

            //_children[_parent[v]].
        }
        public Dictionary<string, Dictionary<string, object>> _children = new Dictionary<string, Dictionary<string, object>>();
        public Dictionary<string, Dictionary<string, object>> _preds = new Dictionary<string, Dictionary<string, object>>();
        public Dictionary<string, Dictionary<string, object>> _sucs = new Dictionary<string, Dictionary<string, object>>();
        public Dictionary<string, Dictionary<string, object>> _in = new Dictionary<string, Dictionary<string, object>>();
        public Dictionary<string, Dictionary<string, object>> _out = new Dictionary<string, Dictionary<string, object>>();
        public static object GRAPH_NODE = " ";

        public Dictionary<string, object> _parent = new Dictionary<string, object>();
        internal object parent(string v)
        {
            if (compound)
            {
                var parent = _parent[v];
                if (parent != GRAPH_NODE)
                {
                    return parent;
                }
            }
            return null;
        }
    }
    public class DagreNode
    {
        public string id;
        public string key;
        public List<SelfEdgeInfo> selfEdges = null;
        public int? rank;
        internal double? x;
        internal double? y;
        internal double? width;
        internal double? height;
        internal string borderTop;
        internal int? minRank;
        internal int? maxRank;
        internal List<object> borderLeft;
        internal List<object> borderRight;
        internal string dummy;
        internal string e;
        internal int? order;
        internal DagreEdge edgeLabel;
        internal string edgeObj;
        internal string _class;

    }
    public class SelfEdgeInfo
    {
        public object label;
        public object e;
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
        public string label;
        public string arrowhead;
        public string id;
        public string labelpos;
        internal bool reversed;
        internal double? x;
        internal double? y;
        internal List<dPoint> points = new List<dPoint>();
        internal double? labelRank;
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
        public string labelpos = "r";
        public int weight = 1;

        public int labeloffset = 10;
        public int minlen = 1;
        internal double? x;
        internal double? y;
        internal List<dPoint> points = new List<dPoint>();
        public int edgesep;
        public int nodesep;
        public string rankdir;
        public int ranksep;
        public string ranker;
        public string acyclicer;
        internal double width;
        internal double height;
        internal int maxRank;
        internal string nestingRoot;
        internal int nodeRankFactor;
        internal List<DagreNode> dummyChains;
        internal string root;
        public string forwardName;
        internal bool reversed;

    }


    public class DagreException : Exception
    {

        public DagreException(string str) : base(str)
        {

        }
    }
}
