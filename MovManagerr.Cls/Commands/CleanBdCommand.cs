namespace MovManagerr.Cls.Commands
{
    public class CleanBdCommand : Command
    {
        public CleanBdCommand() : base("Néttoyer la base de donnée")
        {
        }

        protected override void Start()
        {
            ///var movieCollection = DatabaseHelper.GetInstance<Movie>();
            //movieCollection.DeleteMany(x => true);

        }
    }
}
