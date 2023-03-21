using MovManagerr.Core.Infrastructures.Configurations;
using OpenAI_API;

namespace MovManagerr.Core.Helpers.Extractors
{
    public class OpenAiMovieExtractor : IMovieExtractor
    {
        public FileMovieNameInfo ExtractFromFileName(string fileName)
        {
            var movieInfo = new FileMovieNameInfo()
            {
                MovieName = Path.GetFileNameWithoutExtension(fileName),
                Extensions = Path.GetExtension(fileName)
            };

            OpenAIAPI api = new OpenAIAPI(Preferences.Instance.Settings.OpenAIApiKey); // shorthand

            // create a completion request with a prompt to extract the movie name and year
            string request = "Extract the movie name and year from the following file name: " + fileName;

            // add format to the request
            request += " Format: MovieName: {{movie_name}}, Year: {{year}}";

            try
            {
                string? result = Task.Run<string?>(async () => await api.Completions.GetCompletion(request)).Result;

                if (!string.IsNullOrWhiteSpace(result))
                {
                    movieInfo.MovieName = result.Split(",")[0].Split(":")[1].Trim();
                    movieInfo.Year = 0;

                    try
                    {
                        movieInfo.Year = int.Parse(result.Split(",")[1].Split(":")[1].Trim());               
                    }
                    catch (Exception) { 
                        // l'année est peut être manquante dans le nom de fichié
                    }

                    return movieInfo;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            catch (Exception)
            {
                return movieInfo;
            }
        }
    }
}
