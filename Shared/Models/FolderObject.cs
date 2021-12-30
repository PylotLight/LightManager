namespace LightManager.Shared.Models
{
    public class FolderObject
    {
        public string? Parent { get; set; }
        public IEnumerable<DirectoryObject>? DirectoryObjects { get; set; }
    }

    public class DirectoryObject
    {
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? Type { get; set; }
    }
}