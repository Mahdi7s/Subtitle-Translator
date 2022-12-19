using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace SubtitleTranslator.DataModel
{
    public class StDbContext : DbContext
    {
        public StDbContext(){}
        public StDbContext(string nameOrConnectionString): base(nameOrConnectionString) {}

        public DbSet<Subtitle> Subtitles { get; set; }
        public DbSet<SubtitleMovie> SubtitleMovies { get; set; }
        public DbSet<Dictionary> Dictionaries { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dictionary>().ToTable("Dictionary");
            modelBuilder.Entity<SubtitleMovie>().ToTable("SubtitleMovie");
            modelBuilder.Entity<Subtitle>().Property(x => x.SubtitleMovieId).HasColumnName(
                "SubtitleMovie_SubtitleMovieId");
            modelBuilder.Entity<Subtitle>().ToTable("Subtitle");

            //modelBuilder.Entity<SubtitleMovie>().HasKey(x => x.SubtitleMovieId).Property(x => x.SubtitleMovieId).HasColumnName("RowId");
            //modelBuilder.Entity<Subtitle>().HasKey(x => x.SubtitleId).Property(x => x.SubtitleId).HasColumnName("RowId");

            base.OnModelCreating(modelBuilder);
        }
    }
}
