using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    public abstract class AbstractPreprocessor : IInputPreprocessor
    {
        public AbstractPreprocessor()
        {
            Name = GetType().Name;
            InputSlots = new DataSlot[1] { new DataSlot() { Name = "input" } };
            OutputSlots = new DataSlot[1] { new DataSlot() { Name = "output" } };
        }
        public virtual Type ConfigControl => null;

        public event Action<IInputPreprocessor> PinsChanged;

        public virtual void ParseXml(XElement sb)
        {
        }
        protected void OnPinsChanged()
        {
            PinsChanged?.Invoke(this);
        }
        public DataSlot[] InputSlots { get; set; }
        public DataSlot[] OutputSlots { get; set; }

        public virtual string Name { get; set; }

        public abstract object Process(object input);

        public virtual void StoreXml(StringBuilder sb)
        {

        }
    }
}
