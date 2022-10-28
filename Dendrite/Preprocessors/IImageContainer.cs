using OpenCvSharp;

namespace Dendrite.Preprocessors
{
    public interface IImageContainer
    {
        Mat Image { get; }
    }
}
