using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Dendrite
{
    public class PipelineUI
    {
        public PipelineUI() { }
        public PipelineUI(PipelineGraph gr) { Graph = gr; }
        public List<IUIElement> Elements = new List<IUIElement>();
        PipelineGraph Graph;

        public void Draw(DrawingContext ctx)
        {
            //draw nodes first
            foreach (var item in Elements)
            {
                item.Draw(ctx);
            }
            //draw links
            var ar = Elements.OfType<NodeUI>().SelectMany(z => z.Node.Inputs.Union(z.Node.Outputs)).ToArray();
            var links = ar.SelectMany(z => z.InputLinks.Union(z.OutputLinks)).Distinct().ToArray();
            var nui = Elements.OfType<NodeUI>().ToArray();
            foreach (var item in links)
            {
                var fr = nui.First(z => z.Node == item.Input.Parent);
                var fr2 = nui.First(z => z.Node == item.Output.Parent);
                var pos1 = fr.GetPinPosition(ctx, item.Input);
                var rpos2 = fr2.GetPinPosition(ctx, item.Output);
                var vec = new PointF(pos1.X - rpos2.X, pos1.Y - rpos2.Y);
                var dir = vec.Normalized();
                PointF norm = new PointF(-dir.Y, dir.X);
                var cp1 = vec.Mul(0.25f);
                var cp2 = vec.Mul(0.75f);
                int side = (int)(vec.Length() / 3f);
                var cx = (pos1.X + rpos2.X) / 2;
                
                cp1 = new PointF(rpos2.X + cp1.X + norm.X * side, rpos2.Y + cp1.Y + norm.Y * side);
                cp2 = new PointF(rpos2.X + cp2.X - norm.X * side, rpos2.Y + cp2.Y - norm.Y * side);

                cp1 = new PointF(cx, pos1.Y );
                cp2 = new PointF(cx, rpos2.Y);

                //ctx.Graphics.FillEllipse(Brushes.Red, cp1.X, cp1.Y, 10, 10);
                //ctx.Graphics.FillEllipse(Brushes.Red, cp2.X, cp2.Y, 10, 10);
                ctx.Graphics.DrawBezier(new Pen(Color.Yellow, 2), pos1, cp1, cp2, rpos2);
            }
        }

        public void Init(PipelineGraph pg)
        {
            Graph = pg;
            foreach (var item in Graph.Nodes)
            {
                Elements.Add(new NodeUI() { Node = item });
            }

            //layout
            //topo sort first
            int xx = 0;
            int yy = 100;
            for (int i = 0; i < Elements.Count; i++)
            {
                Elements[i].Position = new PointF(xx, yy);
                xx += 250;
                yy += 150;
            }
        }

        internal void AddItem(Node node)
        {
            Elements.Add(new NodeUI() { Node = node });
        }

        internal void Restore(string epath)
        {
            using (ZipArchive zip = ZipFile.Open(epath, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.Name.EndsWith("ui.xml"))
                    {
                        using (var stream2 = entry.Open())
                        {
                            using (var reader = new StreamReader(stream2))
                            {
                                var config = reader.ReadToEnd();
                                LoadConfig(config);
                            }
                        }
                    }
                }
            }
        }

        private void LoadConfig(string config)
        {
            var doc = XDocument.Parse(config);
            var root = doc.Root;
            Elements.Clear();
            foreach (var item in root.Elements("nodeUI"))
            {
                var node = new NodeUI();
                var nodeId = item.Attribute("nodeId").Value.ParseInt();
                node.Node = Graph.Nodes.First(z => z.Id == nodeId);
                var xx = item.Attribute("x").Value.ParseFloat();
                var yy = item.Attribute("y").Value.ParseFloat();
                var width = item.Attribute("width").Value.ParseFloat();
                var height = item.Attribute("height").Value.ParseFloat();
                node.Position = new PointF(xx, yy);
                node.Width = width;
                node.Height = height;
                Elements.Add(node);
            }
        }

        internal string GetConfigXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<root>");
            foreach (var item in Elements.OfType<NodeUI>())
            {
                sb.AppendLine($"<nodeUI nodeId=\"{item.Node.Id}\" x=\"{item.Position.X.ToDoubleInvariantString()}\" y=\"{item.Position.Y.ToDoubleInvariantString()}\"" +
                    $" width=\"{item.Width.ToDoubleInvariantString()}\" height=\"{item.Height.ToDoubleInvariantString()}\"/>");
            }
            sb.AppendLine("</root>");
            return sb.ToString();
        }
    }
}


