using Dendrite.Lib;
using OpenCvSharp;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using static Dendrite.ImageSourceNode;
using static Onnx.TypeProto.Types;

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
            if (sb.Attribute("useFactor") != null)
            {
                UseFactor = bool.Parse(sb.Attribute("useFactor").Value);
            }
            if (sb.Attribute("nearest") != null)
            {
                NearestInterpolation = bool.Parse(sb.Attribute("nearest").Value);
            }
            if (sb.Attribute("factor") != null)
            {
                Factor = Helpers.ParseDouble(sb.Attribute("factor").Value);
            }
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
            sb.AppendLine($"<resize dims=\"{string.Join(";", Dims)}\" nearest=\"{NearestInterpolation}\" useFactor=\"{UseFactor}\" factor=\"{Factor}\" externalSizeInput=\"{InputSlots.Length == 2}\"/>");
        }

        public void Invalidate()
        {
            OnPinsChanged();
        }
        public ImageSizeFormatTypeEnum SizeFormat { get; set; } = ImageSizeFormatTypeEnum.NCHW;
        public bool UseFactor { get; set; } = false;
        public double Factor { get; set; } = 1.0;
        public bool NearestInterpolation { get; set; }
        public override object Process(object inp)
        {
            var input = InputSlots[0].Data as Mat;
            if (InputSlots.Length == 2)
            {
                Dims = InputSlots[1].Data as int[];
            }
            //var input = inp as Mat;
            Mat ret = null;
            var interpolationMode = NearestInterpolation ? InterpolationFlags.Nearest : InterpolationFlags.Linear;
            if (UseFactor)
                ret = input.Resize(new OpenCvSharp.Size((int)(input.Width * Factor), (int)(input.Height * Factor)), interpolation: interpolationMode);
            else
                ret = input.Resize(new OpenCvSharp.Size(Dims[3], Dims[2]), interpolation: interpolationMode);

            OutputSlots[0].Data = ret;
            OutputSlots[1].Data = new[] { ret.Width, ret.Height };
            if (SizeFormat == ImageSizeFormatTypeEnum.NCHW)
            {
                OutputSlots[1].Data = new int[] { 1, ret.Channels(), ret.Height, ret.Width };
            }
            return ret;
        }
    }
}
