using Onnx;
using System.Collections.Generic;
using System.Linq;

namespace Dendrite
{
    public class GraphNode: ITag
    {
        public GraphNode()
        {
            lock (lock1)
            {
                Id = NewId++;
            }
        }

        public bool DrawHeader = false;
        public float HeaderHeight = 40;


        public static object lock1 = new object();
        public static long NewId = 0;
        public long Id;
        public GraphNode(string opType) : this()
        {
            OpType = opType;
            if (OpType.ToLower().Contains("conv"))
            {
                LayerType = LayerType.Conv;
                return;
            }
            string[] batch = new[] { "batch", "gather", "unsqueeze", "transpose" };
            if (batch.Any(z => OpType.ToLower().Contains(z)))
            {
                LayerType = LayerType.Batch;
            }
            if (OpType.ToLower().Contains("relu"))
            {
                LayerType = LayerType.Relu;
            }
            if (OpType.ToLower().Contains("pad"))
            {
                LayerType = LayerType.Pad;
            }
            if (OpType.ToLower().Contains("transpose"))
            {
                LayerType = LayerType.Transpose;
            }
            if (OpType.ToLower().Contains("softmax"))
            {
                LayerType = LayerType.Softmax;
            }
            if (OpType.ToLower().Contains("pool"))
            {
                LayerType = LayerType.Pool;
            }

            string[] maths = new[] { "add", "matmul", "cast", "shape", "div", "slice" };
            if (maths.Any(z => OpType.ToLower().Contains(z)))
            {
                LayerType = LayerType.MathOperation;
            }

            string[] concats = new[] { "concat", "reshape" };
            if (concats.Any(z => OpType.ToLower().Contains(z)))
            {
                LayerType = LayerType.Concat;
            }
            
        }

        public string Name;
        public List<GraphNode> Childs = new List<GraphNode>();
        public GraphNode Parent;
        public List<GraphNode> Parents = new List<GraphNode>();

        public string OpType;
        public LayerType LayerType;
        public List<InputData> Data = new List<InputData>();
        public List<AttributeInfo> Attributes = new List<AttributeInfo>();
        public object Tag { get; set; }
        public object DrawTag { get; set; }
        public string Input { get; internal set; }
        public long[] Shape;
    }

    public enum LayerType
    {
        Unknown, Conv, Batch, Relu, Input, Output, MathOperation, Concat, Pool, Pad, Softmax, Transpose
    }
}
