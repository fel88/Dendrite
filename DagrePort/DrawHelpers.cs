using System.Drawing;
using System.Drawing.Drawing2D;

namespace Dagre
{
    public static class DrawHelpers
    {
        public static GraphicsPath RoundedRect(RectangleF bounds, int radius)
        {
            int diameter = radius * 2;
            var size = new System.Drawing.Size(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static GraphicsPath HalfRoundedRect(RectangleF bounds, int radius)
        {
            int diameter = radius * 2;
            var size = new System.Drawing.Size(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom   
            arc.Y = bounds.Bottom - diameter;
            path.AddLine(bounds.Right, bounds.Bottom, bounds.Left, bounds.Bottom);


            path.CloseFigure();
            return path;
        }
    }
}
