using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TS7S.Entity;

namespace SubtitleTranslator.DataModel
{
    public class SubtitleRep : Repository<Subtitle>
    {
        private readonly StDbContext _stDbContext;
        public SubtitleRep(StDbContext dbContext) : base(dbContext)
        {
            _stDbContext = dbContext;
        }

        public override void Insert(Subtitle entity)
        {
            var id = !_dbSet.Any() ? 1 : _dbSet.Max(x => x.SubtitleId) + 1;
            ExecuteSqlCommand(
                "Insert Into Subtitle (SubtitleId, SubtitleMovie_SubtitleMovieId, EnWord, PeWord, Paragraph, StartTime, EndTime) Values ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                id, entity.SubtitleMovieId, entity.EnWord, entity.PeWord, entity.Paragraph, entity.StartTime,
                entity.EndTime);
        }
    }
}
