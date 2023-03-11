using System.Text.RegularExpressions;
using TMDbLib.Objects.Search;

namespace MovManagerr.Core.Helpers.Extensions
{
    public static class MovieTmdbExtensions
    {
        public static bool IsValidFolder(this string c)
        {
            Regex regex = new Regex("^([a-zA-Z0-9][^*/><?\"|:]*)$");
            return regex.IsMatch(c);
        }

        public static string GetValidName(this SearchMovie searchMovie)
        {
            if (searchMovie.OriginalTitle.IsValidFolder())
            {
                return searchMovie.OriginalTitle;
            }

            return searchMovie.Title;
        }
    }
}
