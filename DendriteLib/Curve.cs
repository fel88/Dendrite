using System.Drawing;
using System.Drawing.Drawing2D;

namespace Dendrite
{
    public class Curve
    {
        public Curve(PointF[] pnts,PathObject po)
        {
            Path = po;
            for (var i = 0; i < pnts.Length; i++)
            {
                if (i == 0) lineStart();
                point(pnts[i].X, pnts[i].Y);
                if (i == pnts.Length - 1) lineEnd();
            }
        }

        MoveContext context = new MoveContext();
        public PathObject Path = null;
        float _x0;
        float _x1;
        float _y0;
        float _y1;

        int _point;
        int _line;

        void lineStart()
        {
            context.Path = Path;
            _x0 = float.NaN;
            _x1 = float.NaN;
            _y0 = float.NaN;
            _y1 = float.NaN;
            _point = 0;
        }

        void curve(float x, float y)
        {
            PointF p0 = new PointF(_x0, _y0);
            PointF p1 = new PointF(_x1, _y1);

            PointF cp1 = new PointF((2 * p0.X + p1.X) / 3, (2 * p0.Y + p1.Y) / 3);
            PointF cp2 = new PointF((p0.X + 2 * p1.X) / 3, (p0.Y + 2 * p1.Y) / 3);
            PointF cp3 = new PointF((p0.X + 4 * p1.X + x) / 6, (p0.Y + 4 * p1.Y + y) / 6);

            context.bezierCurveTo(cp1, cp2, cp3);
        }

        void lineEnd()
        {
            switch (this._point)
            {
                case 3:
                    this.curve(this._x1, this._y1);
                    context.lineTo(this._x1, this._y1);


                    break;
                case 2:
                    context.lineTo(this._x1, this._y1);
                    break;
            }
            if (this._line != 0 || (this._line != 0 && this._point == 1))
            {
                //Path.CloseFigure();
                //this._context.closePath();
            }
            this._line = 1 - this._line;
        }

        void point(float x, float y)
        {
            x = +x;
            y = +y;
            switch (this._point)
            {
                case 0:
                    this._point = 1;
                    if (this._line != 0)
                    {
                        context.lineTo(x, y);
                    }
                    else
                    {
                        context.moveTo(x, y);
                    }
                    break;
                case 1:
                    this._point = 2;
                    break;
                case 2:
                    this._point = 3;
                    context.lineTo((5 * this._x0 + this._x1) / 6, (5 * this._y0 + this._y1) / 6);
                    this.curve(x, y);
                    break;
                default:
                    this.curve(x, y);
                    break;
            }
            this._x0 = this._x1;
            this._x1 = x;
            this._y0 = this._y1;
            this._y1 = y;
        }
        public class MoveContext
        {
            float _x0;
            float _x1;
            float _y0;
            float _y1;
            public PathObject Path;

            public void bezierCurveTo(PointF cp1, PointF cp2, PointF cp3)
            {
                Path.AddBezier(new PointF(_x1, _y1), cp1, cp2, cp3);
                _x1 = cp3.X;
                _y1 = cp3.Y;
            }
            public void moveTo(float x, float y)
            {
                this._x0 = x;
                this._x1 = x;
                this._y0 = y;
                this._y1 = y;
            }

            internal void lineTo(float x, float y)
            {
                Path.AddLine(new PointF(_x1, _y1), new PointF(x, y));
                _x1 = x;
                _y1 = y;

            }
        }

    }
}
