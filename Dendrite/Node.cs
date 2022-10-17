using Dendrite.Preprocessors;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Dendrite
{
    public class Node
    {
        public object Tag;
        public List<NodePin> Inputs = new List<NodePin>();
        public List<NodePin> Outputs = new List<NodePin>();
        public string Name;

        public Node()
        {

        }

        public Node(XElement item)
        {
            Name = item.Attribute("name").Value;
        }

        protected void StoreBody(StringBuilder sb)
        {
            sb.AppendLine("<tag>");
            if (Tag is IInputPreprocessor ip)
            {
                ip.StoreXml(sb);
            }
            sb.AppendLine("</tag>");
            sb.AppendLine("<inputs>");
            foreach (var item in Inputs)
            {
                item.StoreXml(sb);
            }
            sb.AppendLine("</inputs>");
            sb.AppendLine("<outputs>");
            foreach (var item in Outputs)
            {
                item.StoreXml(sb);
            }
            sb.AppendLine("</outputs>");
        }
        public virtual void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<node name=\"{0}\">");
            StoreBody(sb);

            sb.AppendLine("</node>");
        }

        internal void Detach()
        {
            foreach (var item in Inputs)
            {
                foreach (var iii in item.InputLinks)
                {
                    iii.Input.OutputLinks.Remove(iii);
                }
            }
            foreach (var item in Outputs)
            {
                foreach (var iii in item.OutputLinks)
                {
                    iii.Output.InputLinks.Remove(iii);
                }
            }
        }

        private void PropogateLinks()
        {
            foreach (var item in Outputs)
            {
                foreach (var ol in item.OutputLinks)
                {
                    //copy required?
                    if (item.Data.Data is Mat mat)
                    {
                        ol.Output.Data.Data = mat.Clone();
                    }
                    else
                        ol.Output.Data.Data = item.Data.Data;
                }
            }
        }
        public Exception LastException;
        public virtual void Process()
        {
            LastException = null;
            try
            {
                if (Tag is IInputPreprocessor ip)
                {
                    for (int i = 0; i < Inputs.Count; i++)
                    {
                        ip.InputSlots[i].Data = Inputs[i].Data.Data;
                    }
                    ip.Process(null);
                    for (int i = 0; i < Outputs.Count; i++)
                    {
                        Outputs[i].Data.Data = ip.OutputSlots[i].Data;
                    }
                }
                PropogateLinks();
            }
            catch (Exception ex)
            {
                LastException = ex;
            }
        }
    }
}


