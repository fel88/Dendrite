using Dendrite.Lib;
using System.Collections.Generic;

namespace Dendrite
{
    public class PReLU : NeuralItem
    {

        public PReLU(int nOut)
        {
            Weight = new InternalArray(new[] { nOut });
        }
        public InternalArray Weight;

        public override int SetData(List<InternalArray> arrays)
        {
            Weight = arrays[0];
            return 1;
        }

        public override InternalArray Forward(InternalArray ar1)
        {
            InternalArray ar = ar1.Clone();
            var n = ar1.Shape[0];
            var c = ar1.Shape[1];
            List<double> data = new List<double>();
            int pos0 = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    var img = ar.GetNext2dImageFrom4dArray(ref pos0);
                    for (int z = 0; z < img.Data.Length; z++)
                    {
                        img.Data[z] = img.Data[z] < 0 ? (img.Data[z] * Weight.Data[j]) : img.Data[z];
                    }
                    data.AddRange(img.Data);
                }
            }

            ar.Data = data.ToArray();
            return ar;
        }
    }
}
