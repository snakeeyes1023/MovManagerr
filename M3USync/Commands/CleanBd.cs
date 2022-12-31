﻿using M3USync.Config;
using M3USync.Data;
using M3USync.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Commands
{
    public class CleanBd : Command
    {
        public CleanBd() : base("Néttoyer la base de donnée", "La base de donnée à été néttoyée")
        {
        }

        protected override void Start()
        {
            var movieCollection = DatabaseHelper.GetInstance<Movie>();
            movieCollection.DeleteMany(x => true);

        }
    }
}
