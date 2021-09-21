namespace Dendrite
{
    public abstract class GraphLayout
    {
        public abstract void Layout(GraphModel model);
        public virtual bool FlashHoveredRelatives { get; set; } = true;
        public virtual bool DrawHeadersAllowed { get; set; } = false;
        public virtual bool EdgesDrawAllowed { get; set; } = false;
        

    }
}
