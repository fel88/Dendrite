using OpenCvSharp;

namespace Dendrite
{
    public class SegmentationDetectionInfo : DetectionInfo
    {
        public Mat Mask;//binary mask        
        public int? Class;
        public string Label;
        public bool Visible = true;
    }
}
