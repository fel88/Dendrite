using System;
using System.Collections;
using System.Collections.Generic;

namespace Dendrite
{
    public abstract class NeuralItem
    {

        public NeuralItem Parent;
        public CalcLogItem LogItem;
        public abstract InternalArray Forward(InternalArray ar);

        public void LoadStateDict(string path)
        {
            throw new NotImplementedException();

        }
        public long LastMs;
        public virtual NeuralItem[] Childs { get => null; }

        public virtual int SetData(List<InternalArray> arrays)
        {
            return 0;
        }
    }
}
