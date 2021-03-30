using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Dendrite.Preprocessors
{
    public class DrawInstanceSegmentationPostProcessor : AbstractPreprocessor, IPostDrawer
    {

        public override Type ConfigControl => typeof(InstanceSegmentatorDrawerConfigControl);
        static DrawInstanceSegmentationPostProcessor()
        {
            clrs.Add(Scalar.Red);
            clrs.Add(Scalar.Yellow);
            clrs.Add(Scalar.Green);
            clrs.Add(Scalar.Blue);
            clrs.Add(Scalar.Black);
            clrs.Add(Scalar.White);
            clrs.Add(Scalar.LightGray);
            clrs.Add(Scalar.LightBlue);
            clrs.Add(Scalar.MediumVioletRed);
            clrs.Add(Scalar.Violet);
            clrs.Add(Scalar.BlueViolet);
            clrs.Add(Scalar.OrangeRed);
            clrs.Add(Scalar.Orange);
            clrs.Add(Scalar.Orange);
            clrs.Add(Scalar.Orange);
            clrs.Add(Scalar.Orange);
            clrs.Add(Scalar.Orange);
            clrs.Add(Scalar.Orange);
            clrs.Add(Scalar.Orange);
        }
        public float VisThreshold = 0.4f;


        public PictureBox Pbox;
        public Mat LastMat { get; set; }
        static List<Scalar> clrs = new List<Scalar>();

        internal void Redraw()
        {
            var ret = DrawSegmentationMap(orig, Detections, VisThreshold);
            //var ret = DrawSegmentationMap(net.lastReadedMat, dets, VisThreshold);
            LastMat = ret;
            Pbox.Invoke((Action)(() =>
            {
                Pbox.Image = BitmapConverter.ToBitmap(ret);
            }));
        }

        public bool EnableBoxDraw = true;
        public bool EnableTextDraw = true;

        public SegmentationDetectionInfo[] Detections = null;
        Mat orig;
        public Mat DrawSegmentationMap(Mat mat1, SegmentationDetectionInfo[] detections, float visTresh)
        {
            orig = mat1.Clone();
            Mat mat = mat1.Clone();
            Detections = detections;
            for (int i = 0; i < detections.Length; i++)
            {
                if (!detections[i].Visible) continue;
                Mat m2 = detections[i].Mask.Clone();

                Mat[] rgb = new Mat[3];
                Scalar clr;
                if (i < clrs.Count) clr = clrs[i];
                else clr = clrs.Last();

                for (int j = 0; j < 3; j++)
                {

                    rgb[j] = m2.Clone();
                    //rgb[j].ConvertTo(rgb[j], MatType.CV_32F);


                    rgb[j] *= clr[j];

                }
                Mat merged = new Mat();
                Cv2.Merge(rgb, merged);

                merged = merged.Resize(mat.Size());
                Cv2.AddWeighted(mat, 1, merged, 0.5, 0, mat);
                if (EnableBoxDraw)
                {
                    mat.Rectangle(detections[i].Rect, new OpenCvSharp.Scalar(255, 0, 0), 2);
                }
                if (EnableTextDraw)
                {
                    mat.PutText(detections[i].Label + ": " + Math.Round(detections[i].Conf, 4), new Point(detections[i].Rect.Left, detections[i].Rect.Top), HersheyFonts.HersheyComplex, 1, clr);
                }
            }
            return mat;
        }
        public override object Process(object input)
        {
            var list = input as object[];
            var dets = list.Last(z => z is DetectionInfo[]) as DetectionInfo[];
            var net = list.First(z => z is Nnet) as Nnet;
            var ret = DrawSegmentationMap(net.lastReadedMat, dets.Select(z => z as SegmentationDetectionInfo).ToArray(), VisThreshold);
            LastMat = ret;
            Pbox.Invoke((Action)(() =>
            {
                Pbox.Image = BitmapConverter.ToBitmap(ret);
            }));


            return ret;
        }
    }
}
