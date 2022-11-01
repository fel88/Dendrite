using Dendrite.Lib;
using OpenCvSharp;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "oneHotVec")]
    public class OneHotVectorPostProcessor : AbstractPreprocessor, IImageContainer
    {
        public OneHotVectorPostProcessor()
        {
            InputSlots = new DataSlot[2];
            InputSlots[0] = new DataSlot() { Name = "vector" };
            InputSlots[1] = new DataSlot() { Name = "img" };
        }

        public override string Name => "one hot vector";
        public OneHotVectorType ClassesType { get; set; }
        public enum OneHotVectorType
        {
            ImageNet1000
        }
        public Mat Image => OutputSlots[0].Data as Mat;

        public override object Process(object input)
        {
            var vec = InputSlots[0].Data as InternalArray;
            var img = InputSlots[1].Data as Mat;
            int maxind = 0;
            for (int i = 0; i < vec.Data.Length; i++)
            {
                if (vec.Data[i] > vec.Data[maxind])
                {
                    maxind = i;
                }
            }
            var mat = img.Clone() as Mat;
            var classes = Helpers.ReadResource("imagenet1000");
            var s = classes.Split(new char[] { ':', '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries).ToArray();
            var tt = s[maxind * 2 + 1];

            mat.Rectangle(new OpenCvSharp.Rect(0, 0, mat.Width, 30), Scalar.Black, -1);
            mat.PutText(tt, new OpenCvSharp.Point(0, 20), HersheyFonts.HersheyComplexSmall, 1.0, Scalar.White);
            OutputSlots[0].Data = mat;
            return mat;
        }

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine("<oneHotVec/>");
        }
    }
}
