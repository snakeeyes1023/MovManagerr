using Hangfire;
using M3USync.Data;
using M3USync.Http;
using M3USync.Models;
using Microsoft.VisualBasic;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMDbLib.Objects.Search;

namespace M3USync.Commands
{
    public class SearcherCommand<T> : Command
        where T : Content
    {
        private ContentDownloader _downloader;
        public SearcherCommand(string contentName) : base($"Rechercher un contenue {contentName}", true, false)
        {
            _downloader = ContentDownloader.Instance;
        }

        /// <summary>
        /// Gets the user query.
        /// </summary>
        /// <returns></returns>
        public string GetUserQuery()
        {
            Console.WriteLine("Entrer le contenue désirée");

            string search = string.Empty;

            while (string.IsNullOrEmpty(search))
            {
                search = Console.ReadLine() ?? "";
            }

            return search;
        }

        protected override void Start()
        {
            StartAsync().Wait();
        }

        protected virtual IEnumerable<T> GetCandidate(string query, IEnumerable<T> contentsInDb)
        {
            return contentsInDb.Where(x => x.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase));
        }

        protected void Proceed(T element)
        {
            _downloader.Download(element);
        }

        public async Task StartAsync()
        {
            var moviesInDb = GetAllContentsAsync();

            var query = GetUserQuery();

            var candidates = GetCandidate(query, await moviesInDb);

            T? selected = GetSelectedCandidate(candidates);

            if (selected != null)
            {
                Proceed(selected);
            }
        }

        private static T? GetSelectedCandidate(IEnumerable<T> candidates)
        {
            int count = 0;

            foreach (var item in candidates)
            {
                Console.WriteLine($"{count++} : {item}");
            }

            Console.WriteLine("Entrer le numéro du contenue désiré");

            var selectedNumber = Console.ReadLine() ?? "";

            if (int.TryParse(selectedNumber, out int selected) && selected >= 0 && selected <= candidates.Count())
            {
                return candidates.ElementAt(selected);
            }

            return null;
        }

        public Task<List<T>> GetAllContentsAsync()
        {
            var collection = DatabaseHelper.GetInstance<T>();
            return collection.Find(x => true).ToListAsync();
        }
    }
}
