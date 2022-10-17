using System.IO;

namespace Dendrite
{
    public class DiskFilesystem : IFilesystem
    {
        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }
    }
}


