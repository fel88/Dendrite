using System;

namespace Dendrite.Preprocessors
{
    public abstract class AbstractPreprocessor : IInputPreprocessor
    {
        public virtual Type ConfigControl => null;

        public abstract object Process(object input);

    }
}
