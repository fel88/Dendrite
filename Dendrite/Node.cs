using System.Collections.Generic;

namespace Dendrite
{
    public class Node
    {
        public object Tag;
        public List<NodePin> Inputs = new List<NodePin>();
        public List<NodePin> Outputs = new List<NodePin>();
        public string Name;
    }
}


