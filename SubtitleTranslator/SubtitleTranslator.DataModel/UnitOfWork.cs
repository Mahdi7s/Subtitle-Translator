using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using TS7S.Base.IO;
using TS7S.Entity;

namespace SubtitleTranslator.DataModel
{
    [Export]
    public class UnitOfWork : UnitOfWork<StDbContext>
    {
        private SubtitleRep _subtitleRep = null;
        private SubtitleMovieRep _subtitleMovieRep = null;
        private Repository<Dictionary> _dictionaryRep = null;

        public UnitOfWork() : base(new StDbContext()) { }

        public SubtitleRep SubtitleRep { get { return (_subtitleRep = _subtitleRep ?? new SubtitleRep(_dbContext)); } }
        public SubtitleMovieRep SubtitleMovieRep { get { return (_subtitleMovieRep = _subtitleMovieRep ?? new SubtitleMovieRep(_dbContext)); } }
        public Repository<Dictionary> DictionaryRep { get { return (_dictionaryRep = _dictionaryRep ?? new Repository<Dictionary>(_dbContext)); } }
    }
}
