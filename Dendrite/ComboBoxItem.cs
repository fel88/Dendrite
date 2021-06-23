namespace Dendrite
{
    public class ComboBoxItem
    {
        public string Name { get; set; }
        public object Tag;
        public override string ToString()
        {
            return Name;
        }
    }
}

