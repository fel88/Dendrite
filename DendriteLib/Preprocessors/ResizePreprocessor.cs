using Dendrite.Lib;
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

        public override string Name => "resize";
        //public override Type ConfigControl => typeof(ResizeConfigControl);

        public Mat Image => OutputSlots[0].Data as Mat;

        public int[] Dims = new[] { 1, 3, 256, 256 };
        public override void ParseXml(XElement sb)
        {
            Dims = sb.Attribute("dims").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(Helpers.ParseInt).ToArray();
            if (sb.Attribute("externalSizeInput") != null)
            {
                var b1 = bool.Parse(sb.Attribute("externalSizeInput").Value);
                if (b1)
                {
                    var aa = InputSlots[0];
                    InputSlots = new DataSlot[2];
                    InputSlots[0] = aa;
                    InputSlots[1] = new DataSlot() { Name = "size" };
                    Invalidate();
                }
            }
        }

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<resize dims=\"{string.Join(";", Dims)}\" externalSizeInput=\"{InputSlots.Length == 2}\"/>");
        }

        public void Invalidate()
        {
            OnPinsChanged();
        }

        public override object Process(object inp)
        {
            var input = InputSlots[0].Data as Mat;
            if (InputSlots.Length == 2)
            {
                Dims = InputSlots[1].Data as int[];
            }
            //var input = inp as Mat;
            var ret = input.Resize(new OpenCvSharp.Size(Dims[3], Dims[2]));
            OutputSlots[0].Data = ret;
            OutputSlots[1].Data = new[] { ret.Width, ret.Height };
            return ret;
        }
    }
}
