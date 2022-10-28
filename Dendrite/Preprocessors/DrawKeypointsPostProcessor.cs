using OpenCvSharp;

namespace Dendrite.Preprocessors
{
    public class DrawKeypointsPostProcessor : AbstractPreprocessor, IImageContainer
    {

        public DrawKeypointsPostProcessor()
        {
            InputSlots = new DataSlot[2];
            InputSlots[0] = new DataSlot() { Name = "keypoints" };
            InputSlots[1] = new DataSlot() { Name = "img" };
        }
        public float VisThreshold { get; set; } = 0.4f;

        public static int[] Edges = new int[] {
            0,1,0,2,2,4,1,3,6,8,8,10,5,7,7,9,5,11,11,13,13,15,6,12,12,14,14,16,5,6
        };

        public Mat Image => OutputSlots[0].Data as Mat;

        public override object Process(object input)
        {
            var dets = InputSlots[0].Data as KeypointsDetectionInfo[];
            var img = InputSlots[1].Data as Mat;
            var ret = Helpers.DrawKeypoints(img, dets, VisThreshold);
            OutputSlots[0].Data = ret;
            return ret;
        }
    }
}
