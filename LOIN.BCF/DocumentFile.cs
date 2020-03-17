using System.IO;

namespace LOIN.BCF
{
    public class DocumentFile
    {
        public string Name { get; set; }
        public Stream Stream { get; set; }

        public DocumentFile() { }

        public DocumentFile(string filePath)
        {
            Name = Path.GetFileName(filePath);
            Stream = File.OpenRead(filePath);
        }

        public void WriteTo(Stream target)
        {
            if (Stream.CanSeek) Stream.Seek(0, SeekOrigin.Begin);
            Stream.CopyTo(target);
            Stream.Close();
            Stream.Dispose();
        }
    }
}
