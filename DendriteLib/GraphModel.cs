using System;
using System.Collections.Generic;

namespace Dendrite
{
    public class GraphModel
    {
        public GraphModel()
        {

        }
        public string Name;
        public string Path;

        public ModelProvider Provider;
        public GraphNode[] Nodes;
        public EdgeNode[] Edges;
        public List<GroupNode> Groups = new List<GroupNode>();
        public List<GroupNode> Clusters = new List<GroupNode>();

    }
    public class GroupNode : GraphNode
    {
        //public bool ExpandRequest = false;
        public string Prefix;
        public GraphNode[] Nodes;
    }
}
