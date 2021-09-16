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


            if (gr._edgeCount != _edgeCount) throw new DagreException();
            if (gr._nodeCount != _nodeCount) throw new DagreException();
            if (gr._isDirected != _isDirected) throw new DagreException();
            if (gr._isCompound != _isCompound) throw new DagreException();
            if (gr._isMultigraph != _isMultigraph) throw new DagreException();
            if (_nodesRaw.Keys.Count != gr._nodesRaw.Keys.Count) throw new DagreException();
            if (_in.Keys.Count != gr._in.Keys.Count) throw new DagreException();
            if (_out.Keys.Count != gr._out.Keys.Count) throw new DagreException();
            if (_successors.Keys.Count != gr._successors.Keys.Count) throw new DagreException();
            if (_predecessors.Keys.Count != gr._predecessors.Keys.Count) throw new DagreException();
            if (_edgeObjs.Keys.Count != gr._edgeObjs.Keys.Count) throw new DagreException();
            if (_edgeLabels.Keys.Count != gr._edgeLabels.Keys.Count) throw new DagreException();
            if (_parent.Keys.Count != gr._parent.Keys.Count) throw new DagreException();
            for (int i = 0; i < _parent.Keys.Count; i++)
            {
                if (_parent.Keys.ToArray()[i] != gr._parent.Keys.ToArray()[i]) throw new DagreException();
            }
            for (int i = 0; i < _nodesRaw.Keys.Count; i++)
            {
                var key1 = _nodesRaw.Keys.ToArray()[i];
                if (!gr._nodesRaw.ContainsKey(key1)) throw new DagreException();
            }
            for (int i = 0; i < _nodesRaw.Keys.Count; i++)
            {
                var key1 = _nodesRaw.Keys.ToArray()[i];
                //if (_nodesRaw.Keys.ToArray()[i] != gr._nodesRaw.Keys.ToArray()[i]) throw new DagreException();
                if (!gr._nodesRaw.ContainsKey(key1)) throw new DagreException();
                dynamic node1 = _nodesRaw[key1];
                dynamic node2 = gr._nodesRaw[key1];

                if (node1.Keys.Count != node2.Keys.Count) throw new DagreException();

                foreach (var key in node1.Keys)
                {
                    dynamic val1 = node1[key];
                    dynamic val2 = node2[key];
                    if (val1 is IDictionary<string, object>)
                    {

                    }
                    else
                    {
                        if (val1 != val2) throw new DagreException();
                    }
                }
            }
            for (int i = 0; i < _edgeLabels.Keys.Count; i++)
            {
                var key1 = _edgeLabels.Keys.ToArray()[i];
                if (!gr._edgeLabels.ContainsKey(key1)) throw new DagreException();

                //if (_edgeLabels.Keys.ToArray()[i] != gr._edgeLabels.Keys.ToArray()[i]) throw new DagreException();
                dynamic edge1 = _edgeLabels[_edgeLabels.Keys.ToArray()[i]];
                dynamic edge2 = gr._edgeLabels[gr._edgeLabels.Keys.ToArray()[i]];
                if (edge1.Keys.Count != edge2.Keys.Count) throw new DagreException();
                foreach (var key in edge1.Keys)
                {
                    dynamic val1 = edge1[key];
                    dynamic val2 = edge2[key];
                    if (val1 != val2) throw new DagreException();
                }

            }
            for (int i = 0; i < _edgeObjs.Keys.Count; i++)
            {
                var key1 = _edgeObjs.Keys.ToArray()[i];
                if (!gr._edgeObjs.ContainsKey(key1)) throw new DagreException();

                //if (_edgeLabels.Keys.ToArray()[i] != gr._edgeLabels.Keys.ToArray()[i]) throw new DagreException();
                dynamic edge1 = _edgeObjs[key1];
                dynamic edge2 = gr._edgeObjs[key1];
                if (edge1.Keys.Count != edge2.Keys.Count) throw new DagreException();
                foreach (var key in edge1.Keys)
                {
                    dynamic val1 = edge1[key];
                    dynamic val2 = edge2[key];
                    if (val1 != val2) throw new DagreException();
                }

            }
            if (!_label.Compare(gr._label)) throw new DagreException();

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
        public dynamic nodeRaw(string v) { return _nodesRaw[v]; }

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
            if (!isDirectred && string.CompareOrdinal(v, w) > 0)//v>w
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
                return preds.Union(successors(v)).OrderBy(z => z).ToArray();
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

        public dynamic[] edgesRaw()
        {
            return _edgeObjs.Values.ToArray();
        }
        internal DagreGraph removeEdge(object args)
        {
            return removeEdge(new object[] { args });
        }
        internal DagreGraph removeEdge(object[] args)
        {
            string key = "";
            if (args.Length == 1)
                key = edgeObjToIdRaw(_isDirected, args[0]);
            else if (args.Length == 2)
                key = edgeArgsToIdRaw(_isDirected, args[0], args[1], null);
            else
                key = edgeArgsToIdRaw(_isDirected, args[0], args[1], args[2]);
            if (!_edgeObjs.ContainsKey(key)) return this;
            dynamic edge = _edgeObjs[key];
            if (edge != null)
            {
                var v = edge["v"];
                var w = edge["w"];
                _edgeLabels.Remove(key);
                _edgeObjs.Remove(key);
                Action<object, object> decrementOrRemoveEntry = (map, _k) =>
                 {
                     var k = (string)_k;
                     dynamic d = map;
                     var val = d[k];
                     d[k]--;
                     //if (!--map[k])
                     if (d[k] == 0)
                     {
                         d.Remove(k);
                     }
                 };
                decrementOrRemoveEntry(this._predecessors[(string)w], v);
                decrementOrRemoveEntry(this._successors[(string)v], w);
                _in[(string)w].Remove(key);
                _out[(string)v].Remove(key);
                this._edgeCount--;
            }
            return this;
        }

        internal object setEdgeRaw(object[] args)
        {
            object value = null;
            dynamic arg0 = args[0];
            object v = null;
            object w = null;
            object name = null;
            bool valueSpecified = false;
            if (arg0 != null && !(arg0 is string) && (arg0).ContainsKey("v"))
            {
                v = arg0["v"];
                w = arg0["w"];
                if (arg0.ContainsKey("name"))
                    name = arg0["name"];
                if (args.Length == 2)
                {
                    value = args[1];
                    valueSpecified = true;
                }
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
            if (!this._isDirected && string.CompareOrdinal((string)v, (string)w) > 0)
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
            return nodesRaw().Where(v =>
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

        internal bool hasEdgeRaw(object[] args)
        {
            return edgeRaw(args) != null;
        }

        public Dictionary<string, object> _edgeObjs = new Dictionary<string, object>();

        private Func<object, object, object, object> _defaultEdgeLabelFn;




        //int _edgesCount = 0;

        internal bool hasNode(string v)
        {
            return _nodesRaw.ContainsKey(v);

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
            var tt = v as IDictionary<string, object>;
            var vvv = tt["v"];
            var www = tt["w"];
            string name = null;
            if (tt.ContainsKey("name"))
            {
                name = (string)tt["name"];
            }
            return edgeArgsToIdRaw(isDirectred, vvv, www, name);
        }
        internal dynamic edgeRaw(object v)
        {
            var key = edgeObjToIdRaw(_isDirected, v);
            return _edgeLabels[key];

        }
        internal dynamic edgeRaw(object[] args)
        {
            string key = "";
            if (args.Length == 1)
                key = edgeObjToIdRaw(_isDirected, args[0]);
            else if (args.Length == 2)
                key = edgeArgsToIdRaw(_isDirected, args[0], args[1], null);
            else
                key = edgeArgsToIdRaw(_isDirected, args[0], args[1], args[2]);

            if (!_edgeLabels.ContainsKey(key)) return null;
            return _edgeLabels[key];

        }
        public string edgeArgsToIdRaw(bool isDirected, object v_, object w_, object name)
        {
            var v = "" + v_;
            var w = "" + w_;
            if (!isDirected && string.CompareOrdinal(v, w) > 0)
            {
                var tmp = v;
                v = w;
                w = tmp;
            }
            return v + '\x01' + w + '\x01' + (name == null ? '\x00' : name);
        }
        internal DagreGraph removeNode(string v)
        {
            if (nodesRaw().Contains(v))
            {
                _nodesRaw.Remove(v);
                if (_isCompound)
                {
                    if (_parent.ContainsKey(v))
                    {
                        _children[(string)_parent[v]].Remove(v);
                        //delete this._children[this._parent[v]][v];                    
                        _parent.Remove(v);
                    }
                    foreach (var child in children(v))
                    {
                        setParent(child);
                    }
                    _children.Remove(v);
                }
                foreach (var e in _in[v].Keys)
                {
                    this.removeEdge(new object[] { this._edgeObjs[e] });
                }
                _in.Remove(v);
                _predecessors.Remove(v);

                var keys = _out[v].Keys.ToArray();
                foreach (var e in keys)
                {
                    this.removeEdge(new object[] { this._edgeObjs[e] });
                }
                _out.Remove(v);
                _successors.Remove(v);
                --_nodeCount;
            }
            return this;
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
                if (u == null)
                {
                    return edges;
                }
                return edgesRaw().Where(edge => ((dynamic)edge)["v"] == u).ToArray();
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

        internal void setParent(string v, object parent = null)
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

        public DagreException() { }
        public DagreException(string str) : base(str)
        {

        }
    }
}
