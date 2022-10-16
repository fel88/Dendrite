using System.Collections.Generic;
using System.Drawing;

namespace Dendrite
{
    public class PipelineUI
    {
        public List<IUIElement> Elements = new List<IUIElement>();
        PipelineGraph Graph;
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
            for (int i = 0; i < Elements.Count; i++)
            {
                Elements[i].Position = new PointF(xx, 100);
                xx += 250;
            }
        }
    }
}


