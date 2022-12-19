using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubtitleTranslator.Application.Models
{
    public class PlayingMovieMessage
    {
        public string Title { get; set; }
        public string MoviePath { get; set; }
        public string SubtitlePath { get; set; }

        public override bool Equals(object obj)
        {
            var movieMessage = obj as PlayingMovieMessage;
            return (movieMessage != null && (MoviePath == movieMessage.MoviePath || SubtitlePath == movieMessage.SubtitlePath || Title == movieMessage.Title));
        }
    }
}
