using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Dendrite
{
    public class NetNode : Node
    {
        public string ModelPath;

        public NetNode()
        {
        }

        public NetNode(XElement item) : base(item)
        {
            var netEl = item.Element("net");
            ModelPath = netEl.Element("modelPath").Value;
            Tag = new Nnet();
            Net.Init(ModelPath);
        }
        public override void Process()
        {
            Net.ResetContainer();
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (Inputs[i].Name.EndsWith("dims"))
                {
                    Net.SetDims(Inputs[i - 1].Name, Inputs[i].Data.Data as int[]);
                }
            }
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (!Inputs[i].Name.EndsWith("dims"))
                {
                    Net.SetInputArray(Inputs[i].Name, Inputs[i].Data.Data as float[]);
                }
            }
            Net.Run();
            for (int i = 0; i < Net.OutputDatas.Keys.Count; i++)
            {
                Outputs[i].Data.Data = Net.OutputDatas[Net.OutputDatas.Keys.ToArray()[i]];
            }
            
            base.Process();
        }

        public Nnet Net => Tag as Nnet;
        public override void StoreXml(StringBuilder sb)
        {

            sb.AppendLine($"<netNode name=\"{0}\" >");
            sb.AppendLine($"<modelPath>{ModelPath}</modelPath>");

            StoreBody(sb);
            sb.AppendLine("<net>");
            foreach (var item in Net.InputDatas.Keys)
            {
                sb.AppendLine($"<inputNode key=\"{item}\">");
                /*   sb.AppendLine("<preprocessors>");

                   foreach (var pp in Net.InputDatas[item].Preprocessors)
                   {
                       pp.StoreXml(sb);
                   }
                   sb.AppendLine("</preprocessors>");*/
                sb.AppendLine("</inputNode>");
            }
            /*sb.AppendLine("<postprocessors>");
            foreach (var item in Net.Postprocessors)
            {
                item.StoreXml(sb);
            }
            sb.AppendLine("</postprocessors>"); */
            sb.AppendLine("</net>");
            sb.AppendLine("</netNode>");
        }
    }
}


