using System;

namespace Dendrite.Preprocessors
{
    public interface IInputPreprocessor
    {
        Type ConfigControl { get; }
        object Process(object input);
    }
}
