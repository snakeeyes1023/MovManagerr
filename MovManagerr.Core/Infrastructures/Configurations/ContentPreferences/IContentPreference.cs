namespace MovManagerr.Core.Infrastructures.Configurations.ContentPreferences
{
    public interface IContentPreference
    {
        string BasePath { get; set; }
        string SectionName { get; set; }
        Dictionary<int, string> GenresPath { get; }

        DirectoryManager GetDirectoryManager();
        string GetFolderForGenre(int genreId);
    }
}