using LiteDB;
using M3USync.Data.Abstracts;
using M3USync.Data.Helpers;
using M3USync.Downloaders.Contents;
using M3USync.Infrastructures.Configurations;

namespace M3USync.Commands.Searchers
{
    public class SearcherCommand<T> : Command
        where T : Content
    {
        private ContentDownloaderClient _downloader;
        public SearcherCommand(string contentName) : base($"Rechercher un contenue {contentName}", true, false)
        {
            _downloader = ContentDownloaderClient.Instance;
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
            using (var db = new LiteDatabase(Preferences.Instance._DbPath))
            {
                ILiteCollection<T> collection = DatabaseHelper.GetCollection<T>(db);

                return Task.FromResult(collection.FindAll().ToList());
            }
        }
    }
}
