using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dendrite
{
    public partial class ExpandControl : UserControl
    {
        public ExpandControl()
        {
            InitializeComponent();
        }

        public void SetCaption(string text)
        {
            label1.Text = text;
        }
        public void SetValue(string text)
        {
            textBox1.Text = text;
        }

        public List<ExpandSubItem> SubItems = new List<ExpandSubItem>();

        public bool PlusVisible
        {
            get
            {
                return button1.Visible;
            }
            set
            {
                button1.Visible = value;
            }
        }
        public void AddSubItem(string text, string value, Func<string> fullValue = null)
        {
            var bb = new ExpandSubItem() { Name = text, Value = value };
            SubItems.Add(bb);
            Label lab1 = new Label();
            bb.Label = lab1;
            lab1.Text = text;
            Button btn = new Button() { Text = "S" };
            btn.Tag = fullValue == null ? value : (object)fullValue;
            btn.Click += Btn_Click;
            btn.Width = 30;


            TextBox tb1 = new TextBox();
            btn.Height = tb1.Height;


            bb.Tb = tb1;
            bb.Btn = btn;

            tb1.Text = value;
            tb1.ReadOnly = true;
            lab1.Left = 10;
            tb1.Left = lab1.Right + 10;
            btn.Left = Width - 30;
            lab1.Top = textBox1.Bottom + 10;
            tb1.Top = textBox1.Bottom + 10;


            btn.Top = textBox1.Bottom + 10;

            if (SubItems.Count > 1)
            {
                lab1.Top = SubItems[SubItems.Count - 2].Tb.Bottom + 10;
                tb1.Top = SubItems[SubItems.Count - 2].Tb.Bottom + 10;
                btn.Top = SubItems[SubItems.Count - 2].Tb.Bottom + 10;
            }

            Controls.Add(lab1);
            Controls.Add(tb1);
            Controls.Add(btn);
            btn.BringToFront();
            lab1.Visible = false;
            tb1.Visible = false;
            btn.Visible = false;
            //Height = SubItems.Max(z => z.Label.Bottom);


        }

        private void Btn_Click(object sender, EventArgs e)
        {
            if((sender as Button).Tag is string s)
            {
                Clipboard.SetText(s);
            }
            else
            {
                Clipboard.SetText(((sender as Button).Tag as Func<string>)());              

            }
            MessageBox.Show("Data saved to clipboard!");


        }

        bool collapsed = true;
        public void Switch()
        {
            collapsed = !collapsed;
            foreach (var item in SubItems)
            {
                item.Label.Visible = !collapsed;
                item.Tb.Visible = !collapsed;
                item.Btn.Visible = !collapsed;
            }
            if (!collapsed)
            {
                Height = SubItems.Max(z => z.Label.Bottom);
            }
            else
            {
                Height = label1.Bottom;
            }
            PlusChanged?.Invoke();
        }

        public Action PlusChanged;
        private void button1_Click(object sender, EventArgs e)
        {
            Switch();
        }
    }
    public class ExpandSubItem
    {
        public string Name;
        public string Value;
        public TextBox Tb;
        public Label Label;
        public Button Btn;
    }
}
