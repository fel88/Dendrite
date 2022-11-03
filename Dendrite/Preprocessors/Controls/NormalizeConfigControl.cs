using static Dendrite.Preprocessors.NormalizePreprocessor;

namespace Dendrite.Preprocessors.Controls
{
    [PreprocessorBind(typeof(NormalizePreprocessor))]
    public partial class NormalizeConfigControl : UserControl, IProcessorConfigControl
    {
        public NormalizeConfigControl()
        {
            InitializeComponent();            
        }

        NormalizePreprocessor Proc;
        public void Init(IInputPreprocessor proc)
        {
            Proc = proc as NormalizePreprocessor;
            radioButton1.Checked = Proc.RangeType == NormalizeRangeTypeEnum.ZeroOne;
        }
        
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Proc.RangeType = NormalizeRangeTypeEnum.ZeroOne;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Proc.RangeType = NormalizeRangeTypeEnum.MinusPlusOne;
        }
    }
}
