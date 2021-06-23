namespace Dendrite
{
    public abstract class ModelProvider
    {
        public abstract string SaveDialogFilter { get; }

        public abstract bool IsSuitableFile(string path);
        public abstract GraphModel LoadFromFile(string path);

        public abstract void SaveModel(GraphModel model, string path);
        public abstract void UpdateFloatTensor(GraphModel model, GraphNode parentNode, string name, float[] data, long[] dims);
        public abstract void AppendToOutput(GraphModel model, GraphNode node);
        public abstract void UpdateIntAttributeValue(GraphModel model, GraphNode parentNode, string name, int val);
    }
}
