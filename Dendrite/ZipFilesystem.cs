using System.IO.Compression;

namespace Dendrite
{
    public class ZipFilesystem : IFilesystem
    {
        public ZipFilesystem(string path)
        {
            ZipPath = path;
        }
        public string ZipPath;
        public byte[] ReadAllBytes(string path)
        {
            using (ZipArchive zip = ZipFile.Open(ZipPath, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.Name == path)
                    {
                        using (var stream1 = entry.Open())
                        {
                            var model = stream1.ReadFully();
                            return model;
                        }
                    }
                }
            }
            return null;
        }
    }
}


