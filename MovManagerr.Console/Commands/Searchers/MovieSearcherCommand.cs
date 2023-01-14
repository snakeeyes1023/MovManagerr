using MovManagerr.Core.Data;
using MovManagerr.Core.Infrastructures.Configurations;

namespace MovManagerr.Cls.Commands.Searchers
{
    public class MovieSearcherCommand : SearcherCommand<Movie>
    {
        public MovieSearcherCommand() : base("film")
        {
        }

        public override IEnumerable<Movie> GetCandidate(string query, IEnumerable<Movie> contentsInDb)
        {
            var tmdb = Preferences.GetTmdbInstance();

            var perfectMatchs = tmdb.GetRelatedMovies(query);

            if (perfectMatchs != null)
            {
                foreach (var item in contentsInDb)
                {
                    if (perfectMatchs.Any(x => x?.Id.ToString() == item.TMDBID))
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}
