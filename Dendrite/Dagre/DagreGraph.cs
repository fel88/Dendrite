using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace Dendrite.Dagre
{
    public class DagreGraph
    {
        public DagreGraph()
        {
            _children.Add(GRAPH_NODE as string, new Dictionary<string, object>());
        }


        //public bool compound;
        // public bool directed = true;
        public bool _isDirected = true;
        public bool _isCompound = false;
        public bool _isMultigraph = false;

        public void ClearNulls()
        {
            var ar1 = _parent.Where(z => z.Value == GRAPH_NODE || z.Value == null).Select(z => z.Key).ToArray();
            foreach (var item in ar1)
            {
                _parent.Remove(item);
            }

        }
        public bool Compare(DagreGraph gr)
        {
            ClearNulls();
            gr.ClearNulls();


            if (gr._edgeCount != _edgeCount) return false;
            if (gr._nodeCount != _nodeCount) return false;
            if (gr._isDirected != _isDirected) return false;
            if (gr._isCompound != _isCompound) return false;
            if (gr._isMultigraph != _isMultigraph) return false;
            if (_nodesRaw.Keys.Count != gr._nodesRaw.Keys.Count) return false;
            if (_edgeObjs.Keys.Count != gr._edgeObjs.Keys.Count) return false;
            if (_edgeLabels.Keys.Count != gr._edgeLabels.Keys.Count) return false;
            if (_parent.Keys.Count != gr._parent.Keys.Count) return false;
            for (int i = 0; i < _parent.Keys.Count; i++)
            {
                if (_parent.Keys.ToArray()[i] != gr._parent.Keys.ToArray()[i]) return false;
            }
            for (int i = 0; i < _nodesRaw.Keys.Count; i++)
            {
                if (_nodesRaw.Keys.ToArray()[i] != gr._nodesRaw.Keys.ToArray()[i]) return false;
                dynamic node1 = _nodesRaw[_nodesRaw.Keys.ToArray()[i]];
                dynamic node2 = gr._nodesRaw[gr._nodesRaw.Keys.ToArray()[i]];
                if (node1.Keys.Count != node2.Keys.Count) return false;
                if (node1.ContainsKey("width"))
                {
                    if (node1["width"] != node2["width"]) return false;
                    if (node1["height"] != node2["height"]) return false;
                }
            }
            for (int i = 0; i < _edgeLabels.Keys.Count; i++)
            {
                if (_edgeLabels.Keys.ToArray()[i] != gr._edgeLabels.Keys.ToArray()[i]) return false;
                dynamic edge1 = _edgeLabels[_edgeLabels.Keys.ToArray()[i]];
                dynamic edge2 = gr._edgeLabels[gr._edgeLabels.Keys.ToArray()[i]];
                if (edge1.Keys.Count != edge1.Keys.Count) return false;
                foreach (var key in edge1.Keys)
                {
                    dynamic val1 = edge1[key];
                    dynamic val2 = edge2[key];
                    if (val1 != val2) return false;
                }

            }

            if (!_label.Compare(gr._label)) return false;


            return true;

        }

        public static DagreGraph FromJson(string json)
        {
            DagreGraph ret = new DagreGraph();
            ret.LoadJson(json);
            return ret;
        }

        public void LoadJson(string json)
        {
            _children.Clear();
            JavaScriptSerializer jss = new JavaScriptSerializer();
            var d = jss.Deserialize<dynamic>(json) as Dictionary<string, object>;
            foreach (var item in d)
            {
                switch (item.Key)
                {
                    case "_isMultigraph":

                        {
                            _isMultigraph = (bool)item.Value;
                            break;
                        }
                    case "_isCompound":

                        {
                            _isCompound = (bool)item.Value;
                            break;
                        }
                    case "_isDirected":
                        {
                            _isDirected = (bool)item.Value;
                            break;
                        }
                    case "_label":
                        {
                            var dic = item.Value as Dictionary<string, object>;
                            _label.ranksep = (int)dic["ranksep"];
                            _label.edgesep = (int)dic["edgesep"];
                            _label.nodesep = (int)dic["nodesep"];
                            if (dic.ContainsKey("nodeRankFactor"))
                                _label.nodeRankFactor = (int)dic["nodeRankFactor"];
                            if (dic.ContainsKey("nestingRoot"))
                                _label.nestingRoot = (string)dic["nestingRoot"];
                            _label.rankdir = (string)dic["rankdir"];
                            break;
                        }
                    case "_edgeObjs":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                var dind = new DagreEdgeIndex();
                                var aa = edg.Value as Dictionary<string, object>;
                                _edgeObjs.Add(edg.Key, edg.Value);
                            }
                            break;
                        }
                    case "_nodes":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                _nodesRaw.Add(edg.Key, edg.Value);
                            }
                            break;
                        }
                    case "_out":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                _out.Add(edg.Key, edg.Value as Dictionary<string, object>);
                            }
                            break;
                        }
                    case "_in":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                _in.Add(edg.Key, edg.Value as Dictionary<string, object>);
                            }
                            break;
                        }

                    case "_edgeLabels":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                _edgeLabels.Add(edg.Key, edg.Value as Dictionary<string, object>);
                            }
                            break;
                        }
                    case "_children":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                _children.Add(edg.Key, edg.Value as Dictionary<string, object>);
                            }
                            break;
                        }

                    case "_predecessors":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                _predecessors.Add(edg.Key, edg.Value as Dictionary<string, object>);
                            }
                            break;
                        }
                    case "_nodeCount":
                        {
                            _nodeCount = (int)(item.Value);
                            break;
                        }
                    case "_edgeCount":
                        {
                            _edgeCount = (int)(item.Value);
                            break;
                        }
                    case "_successors":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                _successors.Add(edg.Key, edg.Value as Dictionary<string, object>);
                            }
                            break;
                        }
                }
            }
            ClearNulls();
        }

        public string[] nodes()
        {
            return _nodes2.Keys.ToArray();
        }
        public string[] nodesRaw()
        {
            return _nodesRaw.Keys.ToArray();
        }
        public Dictionary<string, DagreNode> _nodes2 = new Dictionary<string, DagreNode>();
        public Dictionary<string, object> _nodesRaw = new Dictionary<string, object>();
        public List<DagreNode> _nodes = new List<DagreNode>();
        public List<DagreEdge> _edges = new List<DagreEdge>();
        DagreLabel _label = new DagreLabel();
        public DagreLabel graph() { return _label; }

        public DagreNode node(string v) { return _nodes2[v]; }
        public object nodeRaw(string v) { return _nodesRaw[v]; }

        internal bool isMultigraph()
        {
            return _isMultigraph;
        }

        public DagreNode node(DagreNode v) { return node(v.key); }
        public List<DagreEdgeIndex> _edgesIndexes = new List<DagreEdgeIndex>();

        public DagreEdge edge(string v)
        {
            /*replace with dictioanry*/
            return _edges.First(z => z.key == v);
        }

        public static string edgeArgsToId(bool isDirectred, object _v, object _w, object name)
        {
            var v = _v + "";
            var w = _w + "";
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
        public static string EDGE_KEY_DELIM = "\x01";//\x01
        public static string DEFAULT_EDGE_NAME = "\x00";//\x00

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
            var e = edgeArgsToId(_isDirected, v, w, name);
            if (_edgeLabels.ContainsKey(e as string)) return null;
            return _edgeLabels[e as string] as DagreLabel;
            //return _edges[v];
        }

        internal string[] neighbors(string v)
        {
            var preds = predecessors(v);
            if (preds != null)
            {
                return preds.Union(successors(v)).ToArray();
            }
            return null;
        }

        public string[] predecessors(string v)
        {
            if (_predecessors.ContainsKey(v))
            {
                var predsV = _predecessors[v];
                return predsV.Keys.ToArray();
            }
            return null;
        }

        public Dictionary<string, object> _edgeLabels = new Dictionary<string, object>();
        public DagreEdgeIndex[] edges()
        {
            return _edgeObjs.Values.Select(z => z as DagreEdgeIndex).ToArray();
        }

        public object[] edgesRaw()
        {
            return _edgeObjs.Values.ToArray();
        }

        internal void removeEdge(object v, object w = null)
        {
            throw new NotImplementedException();
            //_edgesIndexes.Remove(v);
        }

        internal object setEdgeRaw(object[] args)
        {
            object value = null;
            var arg0 = args[0];
            object v = null;
            object w = null;
            object name = null;
            bool valueSpecified = false;
            if (arg0 != null && ((string)arg0).Contains("v"))
            {

            }
            else
            {
                v = args[0];
                w = args[1];
                if (args.Length > 3)
                    name = args[3];
                if (args.Length > 2)
                {
                    value = args[2];
                    valueSpecified = true;
                }
            }
            v = "" + v;
            w = "" + w;
            if (name != null)
            {
                name = "" + name;
            }
            var e = edgeArgsToId(this._isDirected, v, w, name);
            if (this._edgeLabels.ContainsKey(e))
            {
                if (valueSpecified)
                {
                    this._edgeLabels[e] = value;
                }
                return this;
            }
            if (name != null && !_isMultigraph)
            {
                throw new DagreException("Cannot set a named edge when isMultigraph = false");
            }
            // It didn't exist, so we need to create it.
            // First ensure the nodes exist.
            this.setNodeRaw(v);
            this.setNodeRaw(w);
            this._edgeLabels[e] = valueSpecified ? value : this._defaultEdgeLabelFn(v, w, name);
            v = "" + v;
            w = "" + w;
            if (!this._isDirected && int.Parse((string)v) > int.Parse((string)w))
            {
                var tmp = (string)v;
                v = w;
                w = tmp;
            }
            JavaScriptLikeObject jobj = new JavaScriptLikeObject();
            jobj.AddOrUpdate("v", v);
            jobj.AddOrUpdate("w", w);
            jobj.AddOrUpdate("name", name);
            JavaScriptLikeObject jobj2 = new JavaScriptLikeObject();
            jobj2.AddOrUpdate("v", v);
            jobj2.AddOrUpdate("w", w);

            dynamic edgeObj = name != null ? jobj : jobj2;
            edgeObj.Freeze();
            this._edgeObjs[e] = edgeObj;
            Action<dynamic, dynamic> incrementOrInitEntry = (map, k) =>
            {
                var _map = map as Dictionary<string, object>;
                var _k = k as Dictionary<string, object>;
                if (_map.ContainsKey(k))
                {
                    _map[k]++;
                }
                else
                {
                    _map.Add(k, 1);
                }
            };
            incrementOrInitEntry(this._predecessors[(string)w], v);
            incrementOrInitEntry(this._successors[(string)v], w);
            _in[(string)w][(string)e] = edgeObj;
            _out[(string)v][(string)e] = edgeObj;
            _edgeCount++;
            return this;

        }

        public int _edgeCount;
        internal DagreGraph setEdge(string v, string w, object obj, string ss = null)
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

            var e = edgeArgsToId(_isDirected, v, w, name);
            if (_edgeLabels.ContainsKey(e))
            {
                if (valueSpecified)
                {
                    _edgeLabels[e] = val;

                }
                return this;
            }

            if (ee.name == null && !_isMultigraph)
            {
                throw new DagreException("cannot set a named edge when isMultigraph = false");
            }


            setNode(ee.v);
            setNode(ee.w);
            addOrUpdate(e, _edgeLabels, valueSpecified ? val : _defaultEdgeLabelFn(ee.v, ee.w, ee.name));
            //_edgeLabels[e] = valueSpecified ? val : _defaultEdgeLabelFn(ee.v, ee.w, ee.name);

            var edgeObj = edgeArgsToObj(_isDirected, ee.v, ee.w, ee.name);
            addOrUpdate(e, _edgeObjs, edgeObj);

            incrementOrInitEntry(_predecessors[w], v);
            incrementOrInitEntry(_successors[v], w);
            _in[w][e] = edgeObj;
            _out[v][e] = edgeObj;
            _edgeCount++;
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

        internal bool hasEdge(string u, string v)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> _edgeObjs = new Dictionary<string, object>();

        private Func<object, object, object, object> _defaultEdgeLabelFn;




        //int _edgesCount = 0;

        internal bool hasNode(string v)
        {
            return _nodes2.ContainsKey(v);

        }

        public string[] successors(string v)
        {
            if (_successors.ContainsKey(v))
            {
                var sucsV = _successors[v];
                return sucsV.Keys.ToArray();
            }
            return null;
        }
        internal string[] children(object v = null)
        {
            if (v == null)
            {
                v = GRAPH_NODE;

            }
            if (_isCompound)
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


        /*internal DagreEdgeIndex[] outEdges(string v)
        {
            throw new NotImplementedException();
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


        }*/
        internal object[] outEdges(string v, string w = null)
        {
            if (_out.ContainsKey(v))
            {
                var outV = _out[v].Values.ToArray();
                if (outV != null && outV.Any())
                {
                    if (w == null)
                    {
                        return outV;
                    }
                    return outV.Where((dynamic z) => z["w"] == w).ToArray();
                }

            }
            return new object[] { };


        }

        internal IEnumerable<object> nodeEdges(string v, string w = null)
        {
            var inEdges = this.inEdges(v, w);
            if (inEdges != null)
            {
                return inEdges.Union(outEdges(v, w));
            }
            return null;
        }



        internal void setEdge(string w, string v, object label, Guid guid)
        {
            throw new NotImplementedException();
        }


        internal object edge(object e)
        {
            return edge(e as DagreEdgeIndex);
        }
        public string edgeObjToIdRaw(bool isDirectred, dynamic v)
        {
            var tt = v as Dictionary<string, object>;
            var vvv = tt["v"];
            var www = tt["w"];
            string name = null;
            if (tt.ContainsKey("name"))
            {
                name = (string)tt["name"];
            }
            return edgeArgsToIdRaw(isDirectred, vvv, www, name);
        }
        internal object edgeRaw(object v)
        {
            var key = edgeObjToIdRaw(_isDirected, v);
            return _edgeLabels[key];

        }
        internal object edgeRaw(object[] args)
        {
            string key = "";
            if (args.Length == 1)
                key = edgeObjToIdRaw(_isDirected, args[0]);
            key = edgeArgsToIdRaw(_isDirected, args[0], args[1], args[2]);
            return _edgeLabels[key];

        }
        public string edgeArgsToIdRaw(bool isDirected, object v_, object w_, object name)
        {
            var v = "" + v_;
            var w = "" + w_;
            if (!isDirected && int.Parse(v) > int.Parse(w))
            {
                var tmp = v;
                v = w;
                w = tmp;
            }
            return v + '\x01' + w + '\x01' + (name == null ? '\x00' : name);
        }
        internal void removeNode(string v)
        {
            throw new NotImplementedException();
        }
        internal void removeNode(DagreNode v)
        {
            throw new NotImplementedException();
        }

        internal int nodeCount()
        {
            return _nodeCount;
        }

        internal object[] inEdges(string v, string u = null)
        {
            Dictionary<string, object> inV = null;
            if (_in.ContainsKey(v))
            {
                inV = _in[v];
                var edges = inV.Values.ToArray();
                if (u != null)
                {
                    return edges;
                }
                return edges.Where(edge => (edge as DagreEdgeIndex).v == u).ToArray();
            }
            return null;

        }

        object _defaultNodeLabelFn(object v)
        {
            return null;
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
            if (_isCompound)
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
            addOrUpdate(v as string, _predecessors, new Dictionary<string, object>());
            addOrUpdate(v as string, _out, new Dictionary<string, object>());
            addOrUpdate(v as string, _successors, new Dictionary<string, object>());



            _nodeCount++;
            return this;
        }
        public DagreGraph setNodeRaw(object v, object o2 = null)
        {
            if (_nodesRaw.ContainsKey(v as string))
            {
                if (o2 != null)
                {
                    _nodesRaw[v as string] = o2 as DagreNode;

                }
                return this;
            }
            else
            {
                _nodesRaw.Add(v as string, o2 as DagreNode);
            }


            _nodesRaw[v as string] = (o2 != null ? o2 : _defaultNodeLabelFn(v));
            if (_isCompound)
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
            addOrUpdate(v as string, _predecessors, new Dictionary<string, object>());
            addOrUpdate(v as string, _out, new Dictionary<string, object>());
            addOrUpdate(v as string, _successors, new Dictionary<string, object>());



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
        public static void addOrUpdate(string key, dynamic dic, object obj)
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
            if (!_isCompound)
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

        internal DagreGraph setEdge(DagreEdgeIndex e, object label)
        {
            throw new NotImplementedException();
        }

        void removeFromParentsChildList(string v)
        {
            _children[_parent[v as string] as string].Remove(v as string);

            //_children[_parent[v]].
        }
        public Dictionary<string, Dictionary<string, object>> _children = new Dictionary<string, Dictionary<string, object>>();
        public Dictionary<string, Dictionary<string, object>> _predecessors = new Dictionary<string, Dictionary<string, object>>();
        public Dictionary<string, Dictionary<string, object>> _successors = new Dictionary<string, Dictionary<string, object>>();
        public Dictionary<string, Dictionary<string, object>> _in = new Dictionary<string, Dictionary<string, object>>();
        public Dictionary<string, Dictionary<string, object>> _out = new Dictionary<string, Dictionary<string, object>>();
        public static object GRAPH_NODE = "undefined";

        public Dictionary<string, object> _parent = new Dictionary<string, object>();
        internal object parent(string v)
        {
            if (_isCompound)
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

    public class DagreBase
    {
        internal double? width;
        internal double? height;
        internal double? x;
        internal double? y;

    }
    public class DagreNode : DagreBase
    {
        public string id;
        public string key;
        public List<SelfEdgeInfo> selfEdges = null;
        public int? rank;

        internal string borderTop;
        internal int? minRank;
        internal int? maxRank;
        internal List<object> borderLeft;
        internal List<object> borderRight;
        internal string dummy;
        internal DagreEdgeIndex e;
        internal int? order;
        internal DagreEdge edgeLabel;
        internal string edgeObj;
        internal string _class;
        internal int? low;
        internal int? lim;
        internal object parent;
        internal DagreLabel label;
    }
    public class SelfEdgeInfo
    {
        public object label;
        public object e;
    }
    public class DagreEdge : DagreBase
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

        internal List<dPoint> points = new List<dPoint>();
        internal double? labelRank;
    }

    public class dPoint : DagreBase
    {

    }
    public class DagreEdgeIndex
    {
        public string v;
        public string w;
        public string name;
    }

    public class DagreLabel : DagreBase
    {
        public string labelpos = "r";
        public int weight = 1;

        public int labeloffset = 10;
        public int minlen = 1;

        internal List<dPoint> points = new List<dPoint>();
        public int edgesep;
        public int nodesep;
        public string rankdir;
        public int ranksep;
        public string ranker;
        public string acyclicer;

        internal int maxRank;
        internal string nestingRoot;
        internal int nodeRankFactor;
        internal List<string> dummyChains;
        internal string root;
        public string forwardName;
        internal bool? reversed;
        internal int? cutvalue;
        internal int? labelRank;
        internal object nesingEdge;
        internal double? marginx;
        internal double? marginy;

        internal bool Compare(DagreLabel label)
        {
            if (ranker != label.ranker) return false;
            if (ranksep != label.ranksep) return false;
            if (nestingRoot != label.nestingRoot) return false;

            return true;
        }
    }


    public class DagreException : Exception
    {

        public DagreException(string str) : base(str)
        {

        }
    }

    public class JavaScriptLikeObject
    {
        Dictionary<string, object> dic = new Dictionary<string, object>();
        bool _isFreezed;
        public object this[string key]
        {
            get => dic[key];
            set => dic[key] = value;
        }
        public void Freeze()
        {
            _isFreezed = true;
        }
        public void AddOrUpdate(string key, object val)
        {
            if (_isFreezed) return;
            if (dic.ContainsKey(key))
            {
                dic[key] = val;
                return;
            }
            dic.Add(key, val);
        }

        public override string ToString()
        {
            return "jslo";
        }

    }
}
