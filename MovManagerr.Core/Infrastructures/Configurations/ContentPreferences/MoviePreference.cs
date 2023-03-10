using MovManagerr.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Infrastructures.Configurations.ContentPreferences
{
    public class MoviePreference : ContentPreference
    {
        public MoviePreference()
        {
            GenresPath = new Dictionary<int, string>
            {
                { 28, "Action" },
                { 16, "Animation" },
                { 35, "Comédie" },
                { 99, "Documentaire" },
                { 10751, "Familial" },
                { 14, "Fantastique" },
                { 27, "Horreur" },
            };
        }
        public override string BasePath { get; set; } = "";
    }
}
