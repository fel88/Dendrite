using Dendrite.Lib;
using OpenCvSharp;
using System.Text;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "crop")]
    public class CropPreprocessor : AbstractPreprocessor, IImageContainer
    {
        public CropPreprocessor()
        {
            OutputSlots = new DataSlot[2];
            OutputSlots[0] = new DataSlot() { Name = "cropped" };
            OutputSlots[1] = new DataSlot() { Name = "img_size" };
        }

        public override string Name => "crop";

        public Mat Image => OutputSlots[0].Data as Mat;

        public int[] Region { get; set; } = new[] { 100, 100, 256, 256 };

        public override void ParseXml(XElement sb)
        {
            Region = sb.Attribute("region").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(Helpers.ParseInt).ToArray();
        }

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<crop region=\"{string.Join(";", Region)}\" />");
        }

        public override object Process(object inp)
        {
            var input = InputSlots[0].Data as Mat;

            //var input = inp as Mat;
            var ret = new Mat(input, new Rect(Region[0], Region[1], Region[2], Region[3]));
            OutputSlots[0].Data = ret;
            OutputSlots[1].Data = new[] { ret.Width, ret.Height };
            return ret;
        }
    }
}
