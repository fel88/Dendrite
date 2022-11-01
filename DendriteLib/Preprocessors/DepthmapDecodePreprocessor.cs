//using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Linq;
using System.Text;
//using System.Windows.Forms;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "depthmap")]
    public class DepthmapDecodePreprocessor : AbstractPreprocessor, IImageContainer
    {
        // public override Type ConfigControl => typeof(DepthmapConfigControl);
        public override Type ConfigControl => null;

        public override string Name => "depthmap";

        //public bool StackWithSourceImage = true;
        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<depthmap colormap=\"{Colormap}\"/>");
        }

        public override void ParseXml(XElement sb)
        {
            if (sb.Attribute("colormap") != null)
                Colormap = (ColormapTypes)Enum.Parse(typeof(ColormapTypes), sb.Attribute("colormap").Value);
        }

        public ColormapTypes Colormap { get; set; } = ColormapTypes.Magma;

        public Mat Image => OutputSlots[0].Data as Mat;

        protected virtual Mat GetMap()
        {
            var arr = InputSlots[0].Data as InternalArray;
            Mat mat = new Mat(arr.Shape[2],
                arr.Shape[3], MatType.CV_8UC1,
                arr.Data.Select(z => (byte)(z * 255)).ToArray());
            return mat;
        }

        public override object Process(object input)
        {
            var mat = GetMap();
            Cv2.ApplyColorMap(mat, mat, Colormap);
            OutputSlots[0].Data = mat;
            return mat;
        }
    }
}
