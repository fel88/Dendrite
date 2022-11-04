using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    public interface IInputPreprocessor
    {
        event Action<IInputPreprocessor> PinsChanged;
        Type ConfigControl { get; }
        string Name { get; }
        object Process(object input = null);
        DataSlot[] InputSlots { get; }
        DataSlot[] OutputSlots { get; }
        void StoreXml(StringBuilder sb);
        void ParseXml(XElement sb);
    }
}
