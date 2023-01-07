using M3USync.Data;
using M3USync.Data.Helpers;
using MongoDB.Driver;

namespace M3USync.Commands
{
    public class CleanBdCommand : Command
    {
        public CleanBdCommand() : base("Néttoyer la base de donnée")
        {
        }

        protected override void Start()
        {
            var movieCollection = DatabaseHelper.GetInstance<Movie>();
            movieCollection.DeleteMany(x => true);

        }
    }
}
