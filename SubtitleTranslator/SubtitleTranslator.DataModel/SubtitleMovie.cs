using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubtitleTranslator.DataModel
{
    public class SubtitleMovie
    {
        public int SubtitleMovieId { get; set; }

        public string Title { get; set; }
        public string MoviePath { get; set; }
        public string SubtitlePath { get; set; }

        public virtual List<Subtitle> Subtitles { get; set; }
    }
}
