﻿using M3USync.Infrastructures.Configurations;
using M3USync.Infrastructures.UIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Commands.Validators
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
