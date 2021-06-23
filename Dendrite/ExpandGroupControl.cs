using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Dendrite
{
    public partial class ExpandGroupControl : UserControl
    {
        public ExpandGroupControl()
        {
            InitializeComponent();
            posy = label1.Bottom + 10;
        }

        public void SetCaption(string v)
        {
            label1.Text = v;
        }

        int posy = 0;
        public List<ExpandControl> expands = new List<ExpandControl>();
        public void Clear()
        {
            foreach (var item in expands)
            {
                Controls.Remove(item);
            }
            expands.Clear();
            posy = label1.Bottom + 10;
        }

        public void SetModel(GraphModel model)
        {
            this.model = model;
        }
        GraphModel model;
        string nodeName;
        public ExpandControl AddItem(string text, string value, string nodeName = "")
        {
            this.nodeName = nodeName;
            ExpandControl ec = new ExpandControl();
            ec.PlusChanged = () =>
            {
                posy = label1.Bottom + 10;
                for (int i = 0; i < expands.Count; i++)
                {
                    expands[i].Top = posy;
                    posy += expands[i].Height;
                }
                Height = expands.Max(z => z.Bottom);
            };
            ec.SetModel(model, text,nodeName);
            ec.PlusVisible = true;
            expands.Add(ec);
            ec.Left = 10;
            ec.Top = posy;
            posy += ec.Height;
            ec.Width = Width;
            ec.SetCaption(text);
            ec.SetValue(value);
            Controls.Add(ec);
            Height = expands.Max(z => z.Bottom);
            return ec;
        }
    }
}
