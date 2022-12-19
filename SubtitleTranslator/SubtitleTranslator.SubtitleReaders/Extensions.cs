using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Logic;

namespace SubtitleTranslator.SubtitleReaders
{
    public static class Extensions
    {
        public static SubtitleFrame ToSubtitleFrame(this Paragraph paragraph)
        {
            return new SubtitleFrame
                       {Start = paragraph.StartTime.TimeSpan, End = paragraph.EndTime.TimeSpan, Text = paragraph.Text};
        }
    }
}
