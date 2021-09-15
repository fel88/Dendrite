using Dendrite.Dagre;

namespace Dendrite.Layouts
{
    public class DagreGraphLayout : GraphLayout
    {
        public override void Layout(GraphModel model)
        {
            DagreGraph dg = new DagreGraph();
            int ii = 0;

            foreach (var item in model.Nodes)
            {
                dg._nodes2.Add(ii.ToString(), new DagreNode() { });
                ii++;
            }

            DagreLayout dl = new DagreLayout();
            dl.layout(dg);
        }
    }
}
