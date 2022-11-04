using Dendrite.Preprocessors;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Dendrite
{
    public class Node
    {
        public int Id;
        public static int NewId;
        public object Tag;
        public List<NodePin> Inputs = new List<NodePin>();
        public List<NodePin> Outputs = new List<NodePin>();
        public string Name { get; set; }

        public Node()
        {
            Id = NewId++;
        }

        public Node(XElement item)
        {
            Name = item.Attribute("name").Value;
            Id = int.Parse(item.Attribute("id").Value);
            var inps = item.Element("inputs");
            var outps = item.Element("outputs");
            var tag = item.Element("tag");
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(z => z.GetCustomAttribute(typeof(XmlNameAttribute)) != null).ToArray();
            if (tag.Elements().Any())
            {
                var fr = types.FirstOrDefault(z => ((XmlNameAttribute)z.GetCustomAttribute(typeof(XmlNameAttribute))).XmlKey == tag.Elements().First().Name.LocalName);
                if (fr != null)
                {
                    var proc = Activator.CreateInstance(fr) as IInputPreprocessor;
                    proc.ParseXml(tag.Elements().First());
                    Tag = proc;
                }
            }

            Inputs.Clear();
            foreach (var input in inps.Elements())
            {
                Inputs.Add(new NodePin(input) { Parent = this });

            }
            Outputs.Clear();
            foreach (var pp in outps.Elements())
            {
                Outputs.Add(new NodePin(pp) { Parent = this });
            }

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

        internal void Attach(IInputPreprocessor pp)
        {
            Name = pp.Name;
            Tag = pp;
            pp.PinsChanged += Pp_PinsChanged;
            UpdatePins();
        }

        private void UpdatePins()
        {
            var pp = Tag as IInputPreprocessor;
            Inputs.Clear();
            Outputs.Clear();
            foreach (var zitem in pp.InputSlots)
            {
                Inputs.Add(new NodePin(this, zitem) { Name = zitem.Name });
            }
            foreach (var zitem in pp.OutputSlots)
            {
                Outputs.Add(new NodePin(this, zitem) { Name = zitem.Name });
            }
        }

        private void Pp_PinsChanged(IInputPreprocessor proc)
        {
            UpdatePins();
        }

        public virtual void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<node id=\"{Id}\" name=\"{Name}\">");
            StoreBody(sb);

            sb.AppendLine("</node>");
        }

        public void Detach()
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
                    for (int i = 0; i < Math.Min(Inputs.Count, ip.InputSlots.Length); i++)
                    {
                        ip.InputSlots[i].Data = Inputs[i].Data.Data;
                    }
                    ip.Process();
                    for (int i = 0; i < Math.Min(Outputs.Count, ip.OutputSlots.Length); i++)
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


