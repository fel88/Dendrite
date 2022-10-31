using Dendrite.Preprocessors;
using OpenCvSharp;
using System.Text;
using System.Xml.Linq;

namespace Dendrite
{
    public class ImageSourceNode : Node,IImageContainer
    {
        public ImageSourceNode()
        {
            Name = "image source";
            Outputs.Add(new NodePin(this, new DataSlot()) { Name = "img" });
            Outputs.Add(new NodePin(this, new DataSlot()) { Name = "size" });
        }

        public ImageSourceNode(XElement e) : base(e)
        {

        }

        public Mat SourceMat;
        public bool Is32Float { get; set; } = true;
        public ImageSizeFormatTypeEnum SizeFormat { get; set; } = ImageSizeFormatTypeEnum.NCHW;

        public Mat Image => SourceMat;

        public override void Process()
        {
            var mat = SourceMat.Clone();
            if (Is32Float)
            {
                mat.ConvertTo(mat, MatType.CV_32F);
            }
            Outputs[0].Data.Data = mat;
            Outputs[1].Data.Data = new OpenCvSharp.Size(mat.Width, mat.Height); ;
            if (SizeFormat == ImageSizeFormatTypeEnum.NCHW)
            {
                Outputs[1].Data.Data = new int[] { 1, mat.Channels(), mat.Height, mat.Width };
            }
            base.Process();
        }
        public enum ImageSizeFormatTypeEnum
        {
            WH, NCHW
        }

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<imgSourceNode id=\"{Id}\" name=\"{Name}\" >");
            StoreBody(sb);
            sb.AppendLine("</imgSourceNode>");
        }
    }  
}


