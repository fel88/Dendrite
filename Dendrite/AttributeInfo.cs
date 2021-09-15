using Onnx;
using System;
using System.Collections.Generic;

namespace Dendrite
{
    public class AttributeInfo : ITag
    {
        public string Name;
        public List<float> Floats = new List<float>();
        public AttributeInfoDataType Type;
        public List<Int64> Ints = new List<long>();
        public float FloatData;
        public float IntData;
        public string StringData;
        public List<string> Strings;
        public GraphNode Owner;
        public object Tag { get; set; }

    }
}
