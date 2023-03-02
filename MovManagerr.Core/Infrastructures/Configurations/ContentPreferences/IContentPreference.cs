namespace MovManagerr.Core.Infrastructures.Configurations.ContentPreferences
{
    public interface IContentPreference
    {
        string BasePath { get; set; }
        string SectionName { get; set; }
        Dictionary<int, string> GenresPath { get; }
        string UnfoudedGenreFolder { get; set; }

        DirectoryManager GetDirectoryManager();
        string GetFolderForGenre(int genreId);
    }
}