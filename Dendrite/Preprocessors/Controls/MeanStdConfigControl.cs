using System.Globalization;

namespace Dendrite.Preprocessors.Controls
{
    [PreprocessorBind(typeof(MeanStdPreprocessor))]
    public partial class MeanStdConfigControl : UserControl, IProcessorConfigControl
    {
        public MeanStdConfigControl()
        {
            InitializeComponent();
        }


        public MeanStdPreprocessor Processor;
        private void button12_Click(object sender, EventArgs e)
        {
            textBox1.Text = "104";
            textBox2.Text = "117";
            textBox3.Text = "123";
            textBox4.Text = "1";
            textBox5.Text = "1";
            textBox6.Text = "1";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

            try { Processor.Mean[0] = double.Parse(textBox1.Text.Replace(",", "."), CultureInfo.InvariantCulture); }
            catch (Exception ex)
            {
                textBox1.BackColor = Color.Red;
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

            try { Processor.Std[0] = double.Parse(textBox6.Text.Replace(",","."), CultureInfo.InvariantCulture); }
            catch (Exception ex)
            {
                textBox6.BackColor = Color.Red;
            }
        }

        public void Init(IInputPreprocessor proc)
        {
            Processor = proc as MeanStdPreprocessor;
            textBox1.Text = Processor.Mean[0].ToString();
            textBox2.Text = Processor.Mean[1].ToString();
            textBox3.Text = Processor.Mean[2].ToString();

            textBox6.Text = Processor.Std[0].ToString();
            textBox5.Text = Processor.Std[1].ToString();
            textBox4.Text = Processor.Std[2].ToString();
            
            
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

            try { Processor.Mean[1] = double.Parse(textBox2.Text.Replace(",", "."), CultureInfo.InvariantCulture); }
            catch (Exception ex)
            {
                textBox2.BackColor = Color.Red;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

            try { Processor.Mean[2] = double.Parse(textBox3.Text.Replace(",", "."), CultureInfo.InvariantCulture); }
            catch (Exception ex)
            {
                textBox3.BackColor = Color.Red;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "0.485";
            textBox2.Text = "0.456";
            textBox3.Text = "0.406";

            textBox6.Text = "0.229";
            textBox5.Text = "0.224";
            textBox4.Text = "0.225";
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            try { Processor.Std[1] = double.Parse(textBox5.Text.Replace(",", "."), CultureInfo.InvariantCulture); }
            catch (Exception ex)
            {
                textBox5.BackColor = Color.Red;
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            try { Processor.Std[2] = double.Parse(textBox4.Text.Replace(",", "."), CultureInfo.InvariantCulture); }
            catch (Exception ex)
            {
                textBox4.BackColor = Color.Red;
            }
        }
    }
}
