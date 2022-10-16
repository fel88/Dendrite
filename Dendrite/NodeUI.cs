﻿using System.Drawing;

namespace Dendrite
{
    public class NodeUI : IUIElement
    {
        public Node Node;
        public PointF Position { get; set; }
        public float Width = 140;
        public float Height = 160;

        public void Draw(DrawingContext ctx)
        {
            var pos = ctx.Transform(Position);
            var rr = Helpers.RoundedRect(new RectangleF(pos.X, pos.Y, Width * ctx.zoom, Height * ctx.zoom), (int)(10 * ctx.zoom));
            ctx.Graphics.FillPath(Brushes.Gray, rr);
            var header = Helpers.HalfRoundedRect(new RectangleF(pos.X, pos.Y, Width * ctx.zoom, 20), (int)(10 * ctx.zoom));

            ctx.Graphics.FillPath(Brushes.LightGray, header);
            //ctx.Graphics.FillRectangle(Brushes.Gray, pos.X, pos.Y, Width * ctx.zoom, Height * ctx.zoom);
            var ms = ctx.Graphics.MeasureString(Node.Name, SystemFonts.DefaultFont);
            /*      ctx.Graphics.DrawString(Node.Name, SystemFonts.DefaultFont, Brushes.White, pos.X + rr.GetBounds().Width / 2 - ms.Width / 2,

                      pos.Y + rr.GetBounds().Height / 2 - ms.Height / 2);*/
            ctx.Graphics.DrawString(Node.Name, SystemFonts.DefaultFont, Brushes.Black, pos.X + rr.GetBounds().Width / 2 - ms.Width / 2,

               pos.Y + ms.Height / 2);
            //ctx.Graphics.DrawRectangle(Pens.Black, pos.X, pos.Y, Width * ctx.zoom, Height * ctx.zoom);
            ctx.Graphics.DrawPath(Pens.Black, rr);

            int shy = 50;
            int pinW = 10;
            foreach (var item in Node.Inputs)
            {
                pos = ctx.Transform(Position);
                var rect = new RectangleF(pos.X - ctx.zoom * pinW / 2, pos.Y + shy * ctx.zoom, pinW * ctx.zoom, pinW * ctx.zoom);
                ctx.Graphics.FillEllipse(Brushes.Yellow, rect);
                ctx.Graphics.DrawString(item.Name, SystemFonts.DefaultFont, Brushes.Black,
                    pos.X + ctx.zoom * pinW * 2,
          pos.Y + shy * ctx.zoom);

                shy += 20;

            }
            shy = 50;
            foreach (var item in Node.Outputs)
            {
                pos = ctx.Transform(Position);
                var rect = new RectangleF(pos.X + Width * ctx.zoom - ctx.zoom * pinW / 2, pos.Y + shy * ctx.zoom, pinW * ctx.zoom, pinW * ctx.zoom);
                ctx.Graphics.FillEllipse(Brushes.Yellow, rect);

                var ms2 = ctx.Graphics.MeasureString(item.Name, SystemFonts.DefaultFont);

                ctx.Graphics.DrawString(item.Name, SystemFonts.DefaultFont, Brushes.Black,
                  pos.X - ctx.zoom * (-Width + ms2.Width),
        pos.Y + shy * ctx.zoom);

                shy += 20;
            }
        }
    }
}


