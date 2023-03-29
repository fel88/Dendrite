namespace Dendrite
{
    public static class Extensions
    {
        public static void ShowError(string msg, string caption)
        {
            MessageBox.Show(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static DialogResult ShowQuestion(string msg, string caption, MessageBoxButtons btn = MessageBoxButtons.YesNo)
        {
            return MessageBox.Show(msg, caption, btn, MessageBoxIcon.Question);
        }

        public static void ShowInfo(string msg, string caption)
        {
            MessageBox.Show(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static PointF Normalized(this PointF input)
        {
            var len = input.Length();
            PointF ret = new PointF(input.X / len, input.Y / len);
            return ret;
        }

        public static PointF Mul(this PointF input, float t)
        {
            PointF ret = new PointF(input.X * t, input.Y * t);
            return ret;
        }

        public static float Length(this PointF input)
        {
            var d = input.X * input.X + input.Y * input.Y;
            return (float)Math.Sqrt(d);
        }
        public static List<NeuralItem> GetAllChilds(NeuralItem item, List<NeuralItem> ret = null)
        {
            if (ret == null)
            {
                ret = new List<NeuralItem>();
            }
            if (item.Childs == null)
            {
                ret.Add(item);
            }
            else
            {
                foreach (var citem in item.Childs)
                {
                    GetWeightedAllChilds(citem, ret);
                }
            }
            return ret;
        }
        public static List<NeuralItem> GetWeightedAllChilds(NeuralItem item, List<NeuralItem> ret = null)
        {
            if (ret == null)
            {
                ret = new List<NeuralItem>();
            }
            if (item.Childs == null)
            {
                ret.Add(item);
            }
            else
            {
                foreach (var citem in item.Childs)
                {
                    if (citem is AvgPool2d) continue;
                    GetWeightedAllChilds(citem, ret);
                }
            }
            return ret;
        }

        public static bool AllowParallelProcessing = false;
        public static InternalArray[] ParallelProcess(NeuralItem[] items, InternalArray input)
        {
            InternalArray[] res = new InternalArray[items.Length];
            if (!AllowParallelProcessing)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    res[i] = items[i].Forward(input);
                }
            }
            else
            {
                Parallel.For(0, items.Length, (i) =>
                {
                    res[i] = items[i].Forward(input);
                });
            }
            return res;
        }
        public static void SetStyle(this TextBox textBox,TextBoxStyle style)
        {
            switch (style)
            {
                case TextBoxStyle.Error:
                    textBox.BackColor = Color.Red;
                    textBox.ForeColor= Color.White;
                    break;
                case TextBoxStyle.Default:
                    textBox.BackColor = Color.White;
                    textBox.ForeColor = Color.Black;
                    break;
                case TextBoxStyle.Warning:
                    textBox.BackColor = Color.Yellow;
                    textBox.ForeColor = Color.Blue;
                    break;
            }
        }
    }
}
