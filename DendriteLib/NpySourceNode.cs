using Dendrite.Preprocessors;
using OpenCvSharp;
using System.Text;
using System.Xml.Linq;

namespace Dendrite
{
    public class NpySourceNode : Node
    {
        public NpySourceNode()
        {
            Name = "npy source";
            Outputs.Add(new NodePin(this, new DataSlot()) { Name = "img" });
            Outputs.Add(new NodePin(this, new DataSlot()) { Name = "size" });
        }

        public NpySourceNode(XElement e) : base(e)
        {

        }

        public Mat SourceMat;

        public InternalArray Data;

        public override void Process()
        {
            var mat = Data.Clone();            
            Outputs[0].Data.Data = mat;
            Outputs[1].Data.Data = mat.Shape;            
            base.Process();
        }
        

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<npySourceNode id=\"{Id}\" name=\"{Name}\" >");
            StoreBody(sb);
            sb.AppendLine("</npySourceNode>");
        }
    }
}


