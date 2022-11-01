using Dendrite.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Dendrite
{
    public class PipelineGraph
    {
        public List<Node> Nodes = new List<Node>();

        public void StoreXml(StringBuilder sb)
        {
            sb.AppendLine("<pipeline>");
            foreach (var item in Nodes)
            {
                item.StoreXml(sb);
            }
            sb.AppendLine("</pipeline>");
        }


        // This class represents a directed graph
        // using adjacency list representation
        class Graph
        {

            // No. of vertices
            private int V;

            // Adjacency List as ArrayList
            // of ArrayList's
            private List<List<int>> adj;

            // Constructor
            Graph(int v)
            {
                V = v;
                adj = new List<List<int>>(v);
                for (int i = 0; i < v; i++)
                    adj.Add(new List<int>());
            }

            // Function to add an edge into the graph
            public void AddEdge(int v, int w) { adj[v].Add(w); }

            // A recursive function used by topologicalSort
            void TopologicalSortUtil(int v, bool[] visited,
                                     Stack<int> stack)
            {

                // Mark the current node as visited.
                visited[v] = true;

                // Recur for all the vertices
                // adjacent to this vertex
                foreach (var vertex in adj[v])
                {
                    if (!visited[vertex])
                        TopologicalSortUtil(vertex, visited, stack);
                }

                // Push current vertex to
                // stack which stores result
                stack.Push(v);
            }

            // The function to do Topological Sort.
            // It uses recursive topologicalSortUtil()
            int[] TopologicalSort()
            {
                Stack<int> stack = new Stack<int>();

                // Mark all the vertices as not visited
                var visited = new bool[V];

                // Call the recursive helper function
                // to store Topological Sort starting
                // from all vertices one by one
                for (int i = 0; i < V; i++)
                {
                    if (visited[i] == false)
                        TopologicalSortUtil(i, visited, stack);
                }

                return stack.ToArray();

            }

            public static Node[] Sort(Node[] nodes)
            {
                Graph g = new Graph(nodes.Length);

                foreach (var item in nodes)
                {
                    foreach (var oi in item.Outputs)
                    {
                        foreach (var zz in oi.OutputLinks)
                        {
                            var ind1 = Array.IndexOf(nodes, zz.Input.Parent);
                            var ind2 = Array.IndexOf(nodes, zz.Output.Parent);
                            g.AddEdge(ind1, ind2);
                        }
                    }
                }

                var ret = g.TopologicalSort();
                List<Node> rr = new List<Node>();
                foreach (var item in ret)
                {
                    rr.Add(nodes[item]);
                }

                return rr.ToArray();
            }
        }

        public Node[] Toposort()
        {
            return Graph.Sort(Nodes.ToArray());
        }

        public void RestoreXml(IFilesystem fs, XElement elem)
        {
            Nodes.Clear();
            foreach (var item in elem.Elements())
            {
                if (item.Name.LocalName == "node")
                {
                    var nd = new Node(item);
                    Nodes.Add(nd);
                }
                else
                if (item.Name.LocalName == "netNode")
                {
                    var nd = new NetNode(item, fs);
                    Nodes.Add(nd);
                }
                else
                if (item.Name.LocalName == "imgSourceNode")
                {
                    var nd = new ImageSourceNode(item);
                    Nodes.Add(nd);
                }
            }

            //restore links
            var pins = Nodes.SelectMany(z => z.Inputs.Union(z.Outputs)).ToArray();
            Dictionary<string, PinLink> links = new Dictionary<string, PinLink>();

            foreach (var item in elem.Elements())
            {
                var nodeId = item.Attribute("id").Value.ParseInt();
                var nd = Nodes.First(z => z.Id == nodeId);
                foreach (var pin in item.Descendants("pin"))
                {
                    var pid = pins.First(z => z.Id == pin.Attribute("id").Value.ParseInt());
                    pid.InputLinks.Clear();
                    foreach (var link in pin.Element("inputLinks").Elements("link"))
                    {
                        var id1 = link.Attribute("inputId").Value.ParseInt();
                        var id2 = link.Attribute("outputId").Value.ParseInt();
                        var p1 = pins.First(z => z.Id == id1);
                        var p2 = pins.First(z => z.Id == id2);
                        var key = p1.Id + ";" + p2.Id;
                        var pp = new PinLink() { Input = p1, Output = p2 };

                        if (links.ContainsKey(key))                        
                            pp = links[key];                        
                        else
                            links.Add(key, pp);

                        pid.InputLinks.Add(pp);

                    }
                    pid.OutputLinks.Clear();
                    foreach (var link in pin.Element("outputLinks").Elements("link"))
                    {
                        var id1 = link.Attribute("inputId").Value.ParseInt();
                        var id2 = link.Attribute("outputId").Value.ParseInt();
                        var p1 = pins.First(z => z.Id == id1);
                        var p2 = pins.First(z => z.Id == id2);
                        var key = p1.Id + ";" + p2.Id;
                        var pp = new PinLink() { Input = p1, Output = p2 };

                        if (links.ContainsKey(key))                        
                            pp = links[key];                        
                        else
                            links.Add(key, pp);

                        pid.OutputLinks.Add(pp);

                    }
                }

            }
        }

        public object[] GetOutputs()
        {
            var ret = Nodes.SelectMany(z => z.Outputs).Where(z => z.OutputLinks.Count == 0).Distinct().ToArray();
            return ret.Select(z => z.Data.Data).ToArray();
        }
    }
}


