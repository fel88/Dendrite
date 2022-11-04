using System.Globalization;

namespace Dendrite.Preprocessors.Controls
{
    [PreprocessorBind(typeof(InstanceSegmentationDecodePreprocessor))]
    public partial class InstanceSegmentationDecoderConfigControl : UserControl, IProcessorConfigControl
    {
        public InstanceSegmentationDecoderConfigControl()
        {
            InitializeComponent();
        }

        InstanceSegmentationDecodePreprocessor Proc;

        public void Init(IInputPreprocessor proc)
        {
            Proc = proc as InstanceSegmentationDecodePreprocessor;
            textBox1.Text = Proc.Threshold.ToString();
            textBox2.Text = Proc.MaskThreshold.ToString();
            textBox3.Text = string.Join(Environment.NewLine, Proc.AllowedClasses);

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Proc.Threshold = double.Parse(textBox1.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {

            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Proc.MaskThreshold = double.Parse(textBox2.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {

            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Proc.AllowedClasses = textBox3.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
