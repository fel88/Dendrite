using System;
using System.Text;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    public abstract class AbstractPreprocessor : IInputPreprocessor
    {
        public AbstractPreprocessor()
        {
            Name = GetType().Name;
        }
        public virtual Type ConfigControl => null;

        public virtual void ParseXml(XElement sb)
        {

        }

        public virtual string Name { get; set; }

        public abstract object Process(object input);

        public virtual void StoreXml(StringBuilder sb)
        {

        }
    }
}
