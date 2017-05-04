using bvmfscrapper.exceptions;
using bvmfscrapper.helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace bvmfscrapper.data
{
    public class DataContext : DbContext
    {
        const string CONNSTRING_FILE = "connection.string";
        private static string connectionString = "";

        static DataContext()
        {
            if (File.Exists(CONNSTRING_FILE))
            {
                connectionString = File.ReadAllText(CONNSTRING_FILE);
            }
            else
            {
                throw new NoConnectionFileException();
            }
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(connectionString);
            optionsBuilder.EnableSensitiveDataLogging();
        }

        // Entities
        public DbSet<Empresa> Empresas { get; set; }

        public DbSet<Ticker> Tickers { get; set; }
    }
}
