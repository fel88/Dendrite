using Dagre;
using System;
using System.Collections.Generic;

namespace Dendrite
{
    public abstract class GraphLayout
    {
        public bool VerticalLayout = true;
        public abstract void Layout(GraphModel model);
        public Func<GraphNode, float> GetRenderTextWidth;
        public virtual bool FlashHoveredRelatives { get; set; } = true;
        public virtual bool DrawHeadersAllowed { get; set; } = false;
        public virtual bool EdgesDrawAllowed { get; set; } = false;

        public List<GroupNode> RequestedGroups = new List<GroupNode>();

    }
}
