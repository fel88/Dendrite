using System;
using System.Collections.Generic;

namespace Dendrite
{
    public class NodeInfo
    {
        public bool IsInput;
        public string Name;
        public int[] Dims;
        public Type ElementType;
        public List<string> Tags = new List<string>();
    }
}
