using SBPriceCheckerMvcAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBPriceCheckerMvcAPI.DAL
{
    public class APIBeerContext : DbContext
    {
        public APIBeerContext() : base("dbConn")
        {
        }

        public DbSet<Beer> Beers { get; set; }
        public DbSet<Price> Prices { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));
        }
    }
}
