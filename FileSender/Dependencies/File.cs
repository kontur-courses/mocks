namespace FileSender.Dependencies
{
    public class File
    {
        public File(string name, byte[] content)
        {
            Name = name;
            Content = content;
        }

        public string Name { get; set; }
        public byte[] Content { get; set; }
    }
}