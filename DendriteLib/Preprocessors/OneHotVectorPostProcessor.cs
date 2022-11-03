using Dendrite.Lib;
using System.Text;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "oneHotVec")]
    public class OneHotVectorPostProcessor : AbstractPreprocessor
    {
        public OneHotVectorPostProcessor()
        {
            InputSlots = new DataSlot[1];
            InputSlots[0] = new DataSlot() { Name = "vector" };            
        }

        public List<string> CustomClasses = new List<string>();

        public int LastMaxClass { get; private set; }
        public string LastMaxClassTitle { get; private set; }
        public override string Name => "one hot vector";
        public OneHotVectorType ClassesType { get; set; }

        public enum OneHotVectorType
        {
            ImageNet1000,
            Custom //from inside list
        }       

        public override object Process(object input)
        {
            var vec = InputSlots[0].Data as InternalArray;            
            int maxind = -1;
            double maxval = 0;

            //softmax calc 
            var all = vec.Data.Select(z => Math.Exp(z)).Sum();
            for (int i = 0; i < vec.Data.Length; i++)
            {
                var val = Math.Exp(vec.Data[i]) / all;
                if (maxind == -1 || val > maxval)
                {
                    maxind = i;
                    maxval = val;
                }
            }            

            List<string> rc = new List<string>();
            if (ClassesType == OneHotVectorType.ImageNet1000)
            {
                var classes = Helpers.ReadResource("imagenet1000");
                var s = classes.Split(new char[] { ':', '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries).ToArray();
                for (int i = 1; i < s.Length; i += 2)
                {
                    rc.Add(s[i]);
                }
            }
            else//custom
            {
                rc = CustomClasses.ToList();
            }

            var tt = rc[maxind];

            LastMaxClass = maxind;
            LastMaxClassTitle = tt;

            
            OutputSlots[0].Data = tt;
            return tt;
        }

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<oneHotVec type=\"{ClassesType}\"/>");
        }
    }
}
