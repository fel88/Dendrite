using System;
using System.Collections.Generic;

namespace Dendrite
{
    public class NodeInfo
    {
        public bool IsInput;
        public bool IsOutput=>!IsInput;
        public string Name;
        public int[] Dims;
        public int[] SourceDims;
        public Type ElementType;
        public List<string> Tags = new List<string>();
    }
}
