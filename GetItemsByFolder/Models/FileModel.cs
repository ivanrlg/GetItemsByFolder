namespace GetItemsByFolder.Models
{
    public class FileModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Size { get; set; }        

        public string ExtensionType1 { get; set; }

        public string ExtensionType2 { get; set; }

        public bool Folder { get; set; }

        public byte[] FileArray { get; set; }
    }
}
