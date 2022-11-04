using OpenCvSharp;
using System.Text;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "instanceSegmentationDrawer")]
    public class DrawInstanceSegmentationPostProcessor : AbstractPreprocessor, IImageContainer
    {
        public DrawInstanceSegmentationPostProcessor()
        {
            InputSlots = new DataSlot[2];
            InputSlots[0] = new DataSlot() { Name = "detections" };
            InputSlots[1] = new DataSlot() { Name = "img" };
        }

        public override string Name => "instance segmentation drawer";
        //public override Type ConfigControl => typeof(InstanceSegmentatorDrawerConfigControl);
        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<instanceSegmentationDrawer/>");
        }
        public Mat Image => OutputSlots[0].Data as Mat;

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

        public float VisThreshold { get; set; } = 0.4f;
        
        static List<Scalar> clrs = new List<Scalar>();
        public SegmentationDetectionInfo[] LastDetections;
        public void Redraw()
        {
            var ret = DrawSegmentationMap(orig, LastDetections, VisThreshold);            
        }

        public bool EnableBoxDraw { get; set; } = true;
        public bool EnableTextDraw { get; set; } = true;
                
        Mat orig;
        public Mat DrawSegmentationMap(Mat mat1, SegmentationDetectionInfo[] detections, float visTresh)
        {
            orig = mat1.Clone();
            Mat mat = mat1.Clone();
            
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
                if (mat.Type() != merged.Type())
                {
                    mat.ConvertTo(mat, MatType.CV_8UC3);
                    merged.ConvertTo(merged, MatType.CV_8UC3);
                }

                Cv2.AddWeighted(mat, 1, merged, 0.5, 0, mat);
                if (EnableBoxDraw)
                {
                    mat.Rectangle(detections[i].Rect, new Scalar(255, 0, 0), 2);
                }
                if (EnableTextDraw)
                {
                    mat.PutText($"{detections[i].Label}: {Math.Round(detections[i].Conf, 4)}", new OpenCvSharp.Point(detections[i].Rect.Left, detections[i].Rect.Top), HersheyFonts.HersheyComplex, 1, clr);
                }
            }
            return mat;
        }

        public override object Process(object input)
        {            
            var dets = InputSlots[0].Data  as DetectionInfo[];
            var mat = InputSlots[1].Data as Mat;
            LastDetections = dets.OfType<SegmentationDetectionInfo>().ToArray();
            var ret = DrawSegmentationMap(mat, LastDetections, VisThreshold);
            OutputSlots[0].Data = ret;

            return ret;
        }
    }
}
