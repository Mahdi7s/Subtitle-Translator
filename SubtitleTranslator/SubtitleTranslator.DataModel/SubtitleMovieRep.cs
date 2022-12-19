using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using TS7S.Entity;

namespace SubtitleTranslator.DataModel
{
    public class SubtitleMovieRep : Repository<SubtitleMovie>
    {
        private SubtitleMovie _unknownMovie;
        private readonly StDbContext _stDbContext;

        public SubtitleMovieRep(StDbContext dbContext) : base(dbContext) 
        {
            _stDbContext = dbContext;
        }

        public SubtitleMovie UnknownMovie
        {
            get { return (_unknownMovie = _unknownMovie ?? GetUnknownMovie()); }
        }

        public override void Insert(SubtitleMovie entity)
        {
            var id = !_dbSet.Any() ? 1 : _dbSet.Max(x => x.SubtitleMovieId) + 1;
            ExecuteSqlCommand("Insert Into SubtitleMovie (SubtitleMovieId, Title, MoviePath, SubtitlePath) Values ({0}, {1}, {2}, {3})",
                              id, entity.Title, entity.MoviePath, entity.SubtitlePath);
        }

        private SubtitleMovie GetUnknownMovie()
        {
            var movie = _stDbContext.SubtitleMovies.FirstOrDefault(x => x.Title.Equals("unknown-movie"));
            if (movie == null)
            {
                movie = new SubtitleMovie { Title = "unknown-movie", MoviePath = "unknown-path", SubtitlePath = "unknown-path" };
                Insert(movie);
            }
            return movie;
        }
    }
}
