using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using SubtitleTranslator.Contracts;
using TS7S.Base;


namespace SubtitleTranslator.SubtitleReaders
{
    public class AllSubtitleReader : SubtitleReader
    {
        private readonly Subtitle _subtitle;

        public AllSubtitleReader(string subtitlePath= null, Encoding encoding = null)
        {
            SubtitlePath = subtitlePath;
            SubtitleEncoding = encoding ?? Encoding.Default;
            _subtitle = new Subtitle();
        }

        public SubtitleFormat SubtitleFormat { get; private set; }

        public override void ReadSubtitle()
        {
            Encoding oEncoding;
            SubtitleFormat = _subtitle.LoadSubtitle(SubtitlePath, out oEncoding, SubtitleEncoding);
            SubtitleEncoding = oEncoding;

            SubtitleFrames = _subtitle.Paragraphs.Select(x => x.ToSubtitleFrame()).ToList();
        }
    }
}