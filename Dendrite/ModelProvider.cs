namespace Dendrite
{
    public abstract class ModelProvider
    {
        public abstract bool IsSuitableFile(string path);
        public abstract GraphModel LoadFromFile(string path);

        public abstract void SaveModel(GraphModel model, string path);
        public abstract void UpdateFloatTensor(GraphModel model, GraphNode node, float[] data, int[] dims);

    }
}
