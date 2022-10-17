using System;

namespace Dendrite.Preprocessors
{
    public class DataSlot
    {
        public string Name;
        public object Data;

        public Action<DataSlot> FetchData;
    }
}
