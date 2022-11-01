namespace Dendrite.Preprocessors.Controls
{
    public class PreprocessorBindAttribute : Attribute
    {
        public Type Type { get; private set; }
        public PreprocessorBindAttribute(Type t)
        {
            Type = t;
        }
    }

}
