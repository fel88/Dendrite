using System.Collections.Generic;
using System.Linq;

namespace Dendrite
{
    public class CalcLogItem
    {
        public CalcLogItem() { }

        public CalcLogItem(object t, string k, long e = 0)
        {
            Target = t;
            Key = k;
            ExecutionTime = e;
        }
        /// <summary>
        /// not use in time recalc
        /// </summary>
        public bool IsPassive;
        public object Target;
        public string Key { get; set; }

        public void RecalcTime()
        {
            if (Childs.Length == 0) return;
            foreach (var item in childs)
            {
                item.RecalcTime();
            }
            ExecutionTime = childs.Where(z => !z.IsPassive).Sum(z => z.ExecutionTime);
        }

        public long ExecutionTime { get; set; }

        List<CalcLogItem> childs = new List<CalcLogItem>();

        public CalcLogItem[] Childs
        {
            get { return childs.ToArray(); }
        }

        public CalcLogItem Parent;

        public void AddChild(CalcLogItem l)
        {
            l.Parent = this;
            childs.Add(l);
        }
    }
}