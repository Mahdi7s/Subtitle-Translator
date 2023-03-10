using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    /// <summary>
    /// http://www.whatwg.org/specs/web-apps/current-work/webvtt.html
    /// </summary>
    public class WebVTT : SubtitleFormat
    {

        static readonly Regex RegexTimeCodes = new Regex(@"^-?\d+:-?\d+:-?\d+\.-?\d+\s*-->\s*-?\d+:-?\d+:-?\d+\.-?\d+", RegexOptions.Compiled);
        static readonly Regex RegexTimeCodesShort = new Regex(@"^-?\d+:-?\d+\.-?\d+\s*-->\s*-?\d+:-?\d+\.-?\d+", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".vtt"; }
        }

        public override string Name
        {
            get { return "WebVTT"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string timeCodeFormat = "{0}:{1:00}:{2:00}.{3:000}"; // h:mm:ss.cc
            const string paragraphWriteFormat = "{0} --> {1}{4}{2}{3}{4}";

            var sb = new StringBuilder();
            sb.AppendLine("WEBVTT");
            sb.AppendLine();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string start = string.Format(timeCodeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds);
                string end = string.Format(timeCodeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds);
                string style = string.Empty;
                if (!string.IsNullOrEmpty(p.Extra) && subtitle.Header == "WEBVTT")
                    style = p.Extra;
                sb.AppendLine(string.Format(paragraphWriteFormat, start, end, FormatText(p), style, Environment.NewLine));
            }
            return sb.ToString().Trim();
        }

        private static string FormatText(Paragraph p)
        {
            return p.Text;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            foreach (string line in lines)
            {
                string s = line;
                if (RegexTimeCodesShort.IsMatch(s))
                {
                    s = "00:" + s.Replace("--> ", "--> 00:");
                }

                if (RegexTimeCodes.IsMatch(s))
                {
                    if (p != null)
                    {
                        subtitle.Paragraphs.Add(p);
                        p = null;
                    }
                    try
                    {
                        string[] parts = s.Replace("-->","@").Split("@".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        p = new Paragraph();
                        p.StartTime = GetTimeCodeFromString(parts[0]);
                        p.EndTime = GetTimeCodeFromString(parts[1]);
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                        _errorCount++;
                        p = null;
                    }
                }
                else if (subtitle.Paragraphs.Count == 0 && line.Trim() == "WEBVTT")
                {
                    subtitle.Header = "WEBVTT";
                }
                else if (p != null && line.Trim().Length > 0)
                {
                    string text = line.Trim();
                    text = RemoveFormatting("v", text);
                    text = RemoveFormatting("rt", text);
                    text = RemoveFormatting("ruby", text);
                    text = RemoveFormatting("c", text);
                    text = RemoveFormatting("span", text);
                    p.Text = (p.Text + Environment.NewLine + text).Trim();
                }
            }
            if (p != null)
                subtitle.Paragraphs.Add(p);
            subtitle.Renumber(1);
        }

        private string RemoveFormatting(string tag, string text)
        {
            int indexOfTag = text.IndexOf("<" + tag + " ");
            if (indexOfTag >= 0)
            {
                int indexOfEnd = text.IndexOf(">", indexOfTag);
                if (indexOfEnd > 0)
                {
                    text = text.Remove(indexOfTag, indexOfEnd - indexOfTag + 1);
                }
            }
            return text;
        }

        private static TimeCode GetTimeCodeFromString(string time)
        {
            // hh:mm:ss.mmm
            string[] timeCode = time.Trim().Split (':', '.', ' ');
            return new TimeCode(int.Parse(timeCode[0]),
                                int.Parse(timeCode[1]),
                                int.Parse(timeCode[2]),
                                int.Parse(timeCode[3]));
        }

    }
}
