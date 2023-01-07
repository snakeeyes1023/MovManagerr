using ConsoleTools;
using M3USync.Commands;
using M3USync.Commands.LogCommands;
using M3USync.Commands.Searchers;
using M3USync.Commands.Validators;

namespace M3USync
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var commands = new List<Command>(2);

            commands.Add(new SyncroniserCommand());
            commands.Add(new MovieSearcherCommand());
            commands.Add(new EpisodeSearcherCommand());
            commands.Add(new CleanBdCommand());
            commands.Add(new ConfigValidatorCommand());
            commands.Add(new ShowLogsCommand());

            
            var menu = new ConsoleMenu(args, level: 0);

            foreach (var item in commands)
            {
                menu.Add(item.CommandName, item.Execute);
            }

            menu.Add("Quitter", () => Environment.Exit(0))
            .Configure(config =>
            {
                config.Selector = "--> ";
                config.EnableFilter = true;
                config.Title = "Menu principal";
                config.EnableBreadcrumb = true;
                config.WriteBreadcrumbAction = titles => Console.WriteLine(string.Join(" / ", titles));
            });

            menu.Show();
        }
    }
}