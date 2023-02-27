using MovManagerr.Core.Infrastructures.Configurations;
using OpenAI_API;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TMDbLib.Objects.Search;

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
                //string result = api.Completions.CreateCompletionAsync(new CompletionRequest(request, model: Model.CurieText, temperature: 0.1)).Result.Completions[0].Text;

                string movieName = result.Split(",")[0].Split(":")[1].Trim();
                string year = result.Split(",")[1].Split(":")[1].Trim();

                // set the movie name and year
                movieInfo.MovieName = movieName;

                if (int.TryParse(year, out int yearAsInt))
                {
                    movieInfo.Year = yearAsInt;
                }

                return movieInfo;
            }
            catch (Exception)
            {
                return movieInfo;
            }
        }
    }
}
