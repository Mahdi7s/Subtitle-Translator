using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;

namespace SubtitleTranslator.Application.Utils
{
    public static class SubtitlePathFinder
    {
        private static readonly string[] SupportedSubtitleExtensions;

        static SubtitlePathFinder()
        {
            SupportedSubtitleExtensions = SubtitleFormat.AllSubtitleFormats.Where(x => !string.IsNullOrWhiteSpace(x.Extension)).Select(x => x.Extension).ToArray();
        }

        public static string[] GetSubtitles(string moviePath)
        {
            if(string.IsNullOrWhiteSpace(moviePath))
                throw new ArgumentNullException("moviePath");
            if(!File.Exists(moviePath))
                throw new FileNotFoundException();

            var directory = Path.GetDirectoryName(moviePath);
            var movieName = Path.GetFileNameWithoutExtension(moviePath);

            return SupportedSubtitleExtensions.Select(x => Path.Combine(directory, movieName + x)).Where(File.Exists).ToArray();
        }
    }
}
