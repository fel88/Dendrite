using System.Drawing;
using System.Drawing.Drawing2D;

namespace Dendrite
{
    public interface IDrawingContext
    {
        object GenerateRenderControl(); 

        //Graphics Graphics { get; }
        float zoom { get; set; }
        float sx { get; set; }
        float sy { get; set; }
        Action Redraw { get; set; }
        Action SwapAction { get; set; }

        PointF Transform(PointF p1);
        PointF Transform(double x, double y);
        Rectangle Transform(Rectangle rect);
        RectangleF Transform(RectangleF rect);

        void FitToPoints(PointF[] points, int gap = 0);
        void ResetTransform();
        void ScaleTransform(float zoom1, float zoom2);
        void DrawString(string s1, Font f2, Brush textBrush, float v1, float v2);
        void TranslateTransform(float x, float y);
        MeasureInfo MeasureString(string v, Font f);
        void FillPath(Brush brush, PathObject graphicsPath);
        void DrawPath(Pen borderPen, PathObject graphicsPath);
        void Init(object pictureBox1);
        void Update();
        void Clear(Color white);
        void StopDrag();
        void Swap();
        void AntiAlias(bool v);
        void DrawLine(Pen pen1, PointF pointF1, PointF pointF2);
        void PushMatrix();
        void PopMatrix();
        PathObject RoundedRect(RectangleF rr2, float cornerRadius);
        PathObject HalfRoundedRect(RectangleF headerRect, float cornerRadius);
        PathObject GetRoundedRectangle(RectangleF r, float d);
        PathObject NewPathObject();
    }

}
