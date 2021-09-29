using Onnx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dendrite
{
    public class GraphNode : ITag
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
            string[] squeeze = new[] { "unsqueeze", "squeeze", "transpose" };
            if (squeeze.Any(z => OpType.ToLower().Contains(z)))
            {
                LayerType = LayerType.Squeeze;
            }
            if (OpType.ToLower().Contains("gather"))
            {
                LayerType = LayerType.Gather;
            }
            if (OpType.ToLower().Contains("batch"))
            {
                LayerType = LayerType.Batch;
            }
            if (OpType.ToLower().Contains("relu") || OpType.ToLower().Contains("tanh") || OpType.ToLower().Contains("sigmoid"))
            {
                LayerType = LayerType.Relu;
            }
            if (OpType.ToLower().Contains("pad"))
            {
                LayerType = LayerType.Pad;
            }
            if (OpType.ToLower().Contains("lstm") || OpType.ToLower().Contains("gru"))
            {
                LayerType = LayerType.Lstm;
            }
            if (OpType.ToLower().Contains("transpose"))
            {
                LayerType = LayerType.Transpose;
            }
            if (OpType.ToLower().Contains("softmax"))
            {
                LayerType = LayerType.Softmax;
            }
            if (OpType.ToLower().Contains("log"))
            {
                LayerType = LayerType.Log;
            }
            if (OpType.ToLower().Contains("pool") || OpType.ToLower().Contains("lrn"))
            {
                LayerType = LayerType.Pool;
            }
            if (OpType.ToLower().Contains("dropout"))
            {
                LayerType = LayerType.Dropout;
            }
            if (OpType.ToLower().Contains("gemm"))
            {
                LayerType = LayerType.Gemm;
            }
            if (OpType.ToLower().Contains("constant"))
            {
                LayerType = LayerType.Constant;
            }

            string[] maths = new[] { "matmul", "mul" };
            if (maths.Any(z => OpType.ToLower().Contains(z)))
            {
                LayerType = LayerType.MathOperation;
            }
            string[] pmaths = new[] { "add", "sub", "pow", "sqrt", "reduce", "exp", "cast", "shape", "div", "slice", "ceil", "abs", "sum", "clip", "max", "scan", "compress",
            "mapper","identity","upsample"};
            if (pmaths.Any(z => OpType.ToLower().Contains(z)))
            {
                LayerType = LayerType.PrimitiveMath;
            }

            string[] concats = new[] { "concat", "reshape" };
            if (concats.Any(z => OpType.ToLower().Contains(z)))
            {
                LayerType = LayerType.Concat;
            }

        }

        public string Name;
        public List<GraphNode> Childs = new List<GraphNode>();
        public static bool ExceptionOnDuplicateChild = false;
        public void AttachChild(GraphNode child)
        {
            if (Childs.Contains(child))
            {
                if (ExceptionOnDuplicateChild)
                    throw new ArgumentException("duplicate child");
                else
                    return;
            }
            Childs.Add(child);
            child.Parents.Add(this);

        }
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
        Unknown, Conv, Batch, Squeeze, Relu, Input, Output, MathOperation, PrimitiveMath, Concat, Pool, Pad, Softmax, Transpose, Dropout, Log, Constant, Gemm, Gather, Lstm
    }
}
