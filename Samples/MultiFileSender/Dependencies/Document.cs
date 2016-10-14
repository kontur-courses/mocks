using System;

namespace Samples.MultiFileSender.Dependencies
{
    public class Document
    {
        public Document(string name, byte[] content, DateTime created, string format)
        {
            Name = name;
            Created = created;
            Format = format;
            Content = content;
        }

        public string Name { get; set; }
        public DateTime Created { get; set; }
        public string Format { get; set; }
        public byte[] Content { get; set; }
    }
}