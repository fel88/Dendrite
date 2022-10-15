using System;
using System.Text;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    public interface IInputPreprocessor
    {
        Type ConfigControl { get; }
        string Name { get; }
        object Process(object input);

        void StoreXml(StringBuilder sb);
        void ParseXml(XElement sb);
    }
}
