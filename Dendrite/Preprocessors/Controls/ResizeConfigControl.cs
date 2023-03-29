using Dendrite.Lib;

namespace Dendrite.Preprocessors.Controls
{
    [PreprocessorBind(typeof(ResizePreprocessor))]
    public partial class ResizeConfigControl : UserControl, IProcessorConfigControl
    {
        public ResizeConfigControl()
        {
            InitializeComponent();
        }

        ResizePreprocessor proc;
        public void Init(IInputPreprocessor proc2)
        {
            this.proc = proc2 as ResizePreprocessor;
            textBox1.Text = proc.Dims[3].ToString();
            textBox2.Text = proc.Dims[2].ToString();
            checkBox1.Checked = proc.InputSlots.Length == 2;
            checkBox2.Checked = proc.UseFactor;
            textBox3.Text = proc.Factor.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            proc.Dims[3] = int.Parse(textBox1.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            proc.Dims[2] = int.Parse(textBox2.Text);

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            var aa = proc.InputSlots.ToArray();
            if (checkBox1.Checked)
            {
                if (proc.InputSlots.Length != 2)
                {
                    proc.InputSlots = new DataSlot[2];
                    proc.InputSlots[0] = aa[0];
                    proc.InputSlots[1] = new DataSlot() { Name = "size" };
                    proc.Invalidate();
                }
            }
            else
            {
                if (proc.InputSlots.Length != 1)
                {
                    proc.InputSlots = new DataSlot[1];
                    proc.InputSlots[0] = aa[0];
                    proc.Invalidate();
                }
            }

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                proc.Factor = Helpers.ParseDouble(textBox3.Text);
                textBox3.SetStyle(TextBoxStyle.Default);
            }
            catch (Exception ex)
            {
                textBox3.SetStyle(TextBoxStyle.Error);
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            proc.UseFactor = checkBox2.Checked;
        }
    }

}
