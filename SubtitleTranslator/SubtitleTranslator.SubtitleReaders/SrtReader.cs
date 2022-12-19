using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SubtitleTranslator.SubtitleReaders
{
    public class SrtReader : SubtitleReader
    {
        private readonly Regex _srtRegex = new Regex(
            @"(?<sequence>\d+)\r\n(?<start>\d{2}\:\d{2}\:\d{2},\d{3}) --\> (?<end>\d{2}\:\d{2}\:\d{2},\d{3})\r\n(?<text>[\s\S]*?\r\n\r\n)",
            RegexOptions.Compiled | RegexOptions.ECMAScript);

        protected override List<SubtitleFrame> GetSubtitleFrames(string subtitleContent)
        {
            var retval = new List<SubtitleFrame>();
            var matches = _srtRegex.Matches(subtitleContent);
            foreach (Match match in matches)
            {
                var groups = match.Groups;
                retval.Add(new SubtitleFrame { Start = TimeSpan.Parse(groups["start"].Value.Replace(',', '.')), End = TimeSpan.Parse(groups["end"].Value.Replace(',', '.')), Text = groups["text"].Value });
            }
            return retval;
        }
    }
}
