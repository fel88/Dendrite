using OpenCvSharp;
using System.Text;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "putText")]
    public class PutTextPostProcessor : AbstractPreprocessor, IImageContainer
    {
        public PutTextPostProcessor()
        {
            InputSlots = new DataSlot[2];
            InputSlots[0] = new DataSlot() { Name = "img" };
            InputSlots[1] = new DataSlot() { Name = "text" };
        }

        
        public override string Name => "put text";
        
        public Mat Image => OutputSlots[0].Data as Mat;

        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        public override object Process(object input)
        {
            var img = (InputSlots[0].Data as Mat).Clone();
            var text = InputSlots[1].Data as string;
            
            img.Rectangle(new OpenCvSharp.Rect(OffsetX, OffsetY, img.Width, 30), Scalar.Black, -1);
            img.PutText(text, new OpenCvSharp.Point(OffsetX, OffsetY+20), HersheyFonts.HersheyComplexSmall, 1.0, Scalar.White);
            OutputSlots[0].Data = img;
            return img;
        }

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine("<putText/>");
        }
    }
}
