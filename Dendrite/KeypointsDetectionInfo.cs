using OpenCvSharp;

namespace Dendrite
{
    public class KeypointsDetectionInfo: DetectionInfo
    {
        public Point2f[] Points;        
    }

    public class DetectionInfo
    {
        public float Conf;
        public Rect Rect;
    }
}
