namespace MovManagerr.Models
{
    public class MovieDirectory
    {
        public MovieDirectory(string title, string path, string year)
        {
            Title = title;
            Path = path;
            Year = int.Parse(year);
        }

        public string Path { get; set; } = "";
        public string Title { get; set; } = "";
        public int Year { get; set; } = 1950;

    }
}