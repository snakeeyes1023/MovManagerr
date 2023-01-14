using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Loggers;

namespace MovManagerr.Cls.Commands.Validators
{
    public class ConfigValidatorCommand : Command
    {
        public ConfigValidatorCommand() : base("Valider les configurations")
        {
        }

        protected override void Start()
        {
            try
            {
                Preferences.RebuildInstance();

                var config = Preferences.Instance;

                config.ValidateConfiguration();

                config.VerifyDriveAccessibility();

                SimpleLogger.AddLog("Vos configuration son valide.");
            }
            catch (Exception ex)
            {
                SimpleLogger.AddLog("La configuration n'est pas valide.");
                SimpleLogger.AddLog(ex.Message);
            }
        }
    }
}
