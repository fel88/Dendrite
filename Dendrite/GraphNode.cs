using System.Collections.Generic;

namespace Dendrite
{
    public class GraphNode
    {
        public string Name;
        public List<GraphNode> Childs = new List<GraphNode>();
        public GraphNode Parent;
        public string OpType;
        public List<InputData> Data = new List<InputData>();
        public List<AttributeInfo> Attributes = new List<AttributeInfo>();
        public object Tag { get; set; }
        public object DrawTag { get; set; }
        public string Input { get; internal set; }
    }
}
