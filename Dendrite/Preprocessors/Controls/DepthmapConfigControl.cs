namespace Dendrite.Preprocessors.Controls
{
    [PreprocessorBind(typeof(DepthmapDecodePreprocessor))]
    public partial class DepthmapConfigControl : UserControl, IProcessorConfigControl
    {
        public DepthmapConfigControl()
        {
            InitializeComponent();
        }
        DepthmapDecodePreprocessor Proc;
        public void Init(IInputPreprocessor proc)
        {
            Proc = proc as DepthmapDecodePreprocessor;
            //checkBox1.Checked = Proc.StackWithSourceImage;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
           // Proc.StackWithSourceImage = checkBox1.Checked;
        }
    }
}
