using Dendrite.Preprocessors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Dendrite
{
    public class NodePin
    {
        public NodePin(Node parent, DataSlot slot)
        {
            Parent = parent;
            Id = NewId++;
            Data = slot;
        }

        public NodePin(XElement el)
        {
            Id = int.Parse(el.Attribute("id").Value);
            Name = el.Attribute("name").Value;
            Data = new DataSlot();
        }

        public int Id { get; private set; }
        public static int NewId;
        public DataSlot Data;
        public string Name;
        public Node Parent;
        public NodePinType Type;
        public List<PinLink> OutputLinks = new List<PinLink>();
        public List<PinLink> InputLinks = new List<PinLink>();

        public void RestoreLinks(XElement el, InferenceEnvironment env)
        {

        }
        internal void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<pin id=\"{Id}\" name=\"{Name}\">");
            sb.AppendLine($"<inputLinks>");
            foreach (var il in InputLinks)
            {
                sb.AppendLine($"<link inputId=\"{il.Input.Id}\" outputId=\"{il.Output.Id}\"/>");
            }
            sb.AppendLine($"</inputLinks>");
            sb.AppendLine($"<outputLinks>");
            foreach (var il in OutputLinks)
            {
                sb.AppendLine($"<link inputId=\"{il.Input.Id}\" outputId=\"{il.Output.Id}\"/>");
            }
            sb.AppendLine($"</outputLinks>");
            sb.AppendLine("</pin>");
        }
    }
}


