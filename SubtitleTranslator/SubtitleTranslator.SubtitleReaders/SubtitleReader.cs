using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SubtitleTranslator.Contracts;
using TS7S.Base;

namespace SubtitleTranslator.SubtitleReaders
{
    public abstract class SubtitleReader : ISubtitleReader
    {
        protected SubtitleReader()
        {
            SubtitleEncoding = Encoding.Default;
        }

        public Encoding SubtitleEncoding { get; set; }
        public List<SubtitleFrame> SubtitleFrames { get; protected set; } 
        public string SubtitlePath { get; set; }

        protected virtual List<SubtitleFrame> GetSubtitleFrames(string subtitleContent)
        {
            throw new NotImplementedException();
        }

        public virtual void ReadSubtitle()
        {
            var content = GetSubtitleContent();
            if (string.IsNullOrEmpty(content))
                throw new Exception("An error occured while reading the subtitle.");

            SubtitleFrames = GetSubtitleFrames(content);
        }

        public virtual string SubtitleOf(TimeSpan time)
        {
            var frame = SubtitleDetailsOf(time);
            return frame != null ? frame.Text : string.Empty;
        }


        private string GetSubtitleContent()
        {
            if (File.Exists(SubtitlePath))
            {
                using (var fstrm = new FileStream(SubtitlePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new StreamReader(fstrm, SubtitleEncoding))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            return null;
        }


        public ISubtitleFrame SubtitleDetailsOf(TimeSpan time)
        {
            if (!SubtitleFrames.IsNullOrEmpty())
            {
                var frame = SubtitleFrames.FirstOrDefault(x => x.Start <= time && x.End >= time);
                return frame;
            }
            return null;
        }
    }

    public sealed class SubtitleFrame : ISubtitleFrame
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public string Text { get; set; }
    }
}
