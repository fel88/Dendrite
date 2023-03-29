using System.IO;
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

        public NetNode(XElement item, IFilesystem fs) : base(item)
        {
            //var netEl = item.Element("net");
            ModelPath = item.Element("modelPath").Value;
            Tag = new Nnet();
            Net.Init(fs, ModelPath);
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
                    if (Inputs[i].Data.Data is float[] ff)
                        Net.SetInputArray(Inputs[i].Name, ff);
                    else if(Inputs[i].Data.Data is InternalArray ar)
                        Net.SetInputArray(Inputs[i].Name, ar.ToFloatArray());
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

            sb.AppendLine($"<netNode id=\"{Id}\" name=\"{Name}\" >");
            var fi = new FileInfo(ModelPath).Name;
            sb.AppendLine($"<modelPath>{fi}</modelPath>");

            StoreBody(sb);

            sb.AppendLine("</netNode>");
        }
    }
}


