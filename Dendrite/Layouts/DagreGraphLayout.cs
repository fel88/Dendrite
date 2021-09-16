using Dendrite.Dagre;

namespace Dendrite.Layouts
{
    public class DagreGraphLayout : GraphLayout
    {
        public override void Layout(GraphModel model)
        {
            DagreGraph dg = new DagreGraph(false);
            int ii = 0;

            foreach (var item in model.Nodes)
            {
                dg._nodesRaw.Add(ii.ToString(), new DagreNode() { });
                ii++;
            }

            DagreLayout dl = new DagreLayout();
            dl.layout(dg);
        }
    }
}
