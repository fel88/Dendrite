using Dendrite.Preprocessors;
using System.Collections.Generic;

namespace Dendrite
{
    public class InputInfo
    {
        public List<IInputPreprocessor> Preprocessors = new List<IInputPreprocessor>();
        public object Data;
    }
}
