using System.Text;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "argMax")]

    public class ArgMaxProcessor : AbstractPreprocessor
    {
        public override string Name => "argMax";

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine("<argMax/>");
        }

        public override Type ConfigControl => null;


        public override object Process(object inp)
        {
            //var list = inp as object[];
            var ar = InputSlots[0].Data as InternalArray;
            var dd = ar.ToFloatArray();
            byte[] outp = new byte[ar.Shape[3] * ar.Shape[2]];
            byte b1 = 0;
            byte b2 = 255;

            int shift = ar.Shape[3] * ar.Shape[2];
            if (ar.Shape[1] != 2)
            {
                throw new NotImplementedException("channels!=2");
            }
            for (int i = 0; i < dd.Length / 2; i++)
            {
                if (dd[i] > dd[i + shift])
                {
                    outp[i] = b1;
                }
                else
                {
                    outp[i] = b2;
                }
            }

            InternalArray ret = new InternalArray(new int[] { 1, 1, ar.Shape[2], ar.Shape[3] });
            ret.Data = outp.Select(z => (double)z).ToArray();
            OutputSlots[0].Data = ret;
            return ret;
        }
    }

}
