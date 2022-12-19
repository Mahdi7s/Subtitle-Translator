using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SubtitleTranslator.DataModel
{
    public class Subtitle
    {
        public int SubtitleId { get; set; }

        public int SubtitleMovieId { get; set; }

        public string EnWord { get; set; }
        public string PeWord { get; set; }
        public string Paragraph { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public SubtitleMovie SubtitleMovie { get; set; }
    }
}
