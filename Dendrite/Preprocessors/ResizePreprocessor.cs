using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "resize")]
    public class ResizePreprocessor : AbstractPreprocessor, IImageContainer
    {

        public ResizePreprocessor()
        {
            OutputSlots = new DataSlot[2];
            OutputSlots[0] = new DataSlot() { Name = "resized_img" };
            OutputSlots[1] = new DataSlot() { Name = "img_size" };
        }
        public override Type ConfigControl => typeof(ResizeConfigControl);

        public Mat Image
        {
            get
            {                
                return OutputSlots[0].Data as Mat;
            }
        }

        public int[] Dims;
        public override void ParseXml(XElement sb)
        {
            Dims = sb.Attribute("dims").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(Helpers.ParseInt).ToArray();
        }

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<resize dims=\"{string.Join(";", Dims)}\"/>");
        }

        public override object Process(object inp)
        {
            var input = InputSlots[0].Data as Mat;
            //var input = inp as Mat;
            var ret = input.Resize(new OpenCvSharp.Size(Dims[3], Dims[2]));
            OutputSlots[0].Data = ret;
            OutputSlots[1].Data = new[] { ret.Width, ret.Height };
            return ret;
        }
    }
}
