using Dendrite.Preprocessors;

namespace Dendrite
{
    public interface IProcessorConfigControl
    {
        void Init(IInputPreprocessor proc);
    }
}
