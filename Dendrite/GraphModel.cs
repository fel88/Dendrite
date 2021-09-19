using System;

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
    }
}
