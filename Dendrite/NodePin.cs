using System.Collections.Generic;

namespace Dendrite
{
    public class NodePin
    {
        public NodePin(Node parent)
        {
            Parent = parent;
        }

        public string Name;
        public Node Parent;
        public NodePinType Type;
        public List<PinLink> OutputLinks = new List<PinLink>();
        public List<PinLink> InputLinks = new List<PinLink>();
    }
}


