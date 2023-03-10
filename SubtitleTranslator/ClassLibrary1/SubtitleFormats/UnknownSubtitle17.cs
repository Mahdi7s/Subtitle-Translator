using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class UnknownSubtitle17 : SubtitleFormat
    {

       static readonly Regex RegexTimeCode = new Regex(@"^\d\d:\d\d:\d\d:\d\d", RegexOptions.Compiled);
       static readonly Regex RegexNumber = new Regex(@"^\[\d+\]$", RegexOptions.Compiled);

        enum ExpectingLine
        {
            Number,
            TimeStart,
            TimeEnd,
            Text
        }

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 17"; }
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
            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);
            string s = sb.ToString();
            if (!s.Contains("[HEADER]") || !s.Contains("[BODY]"))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //[1]
            //01:00:21:20
            //01:00:23:17
            //[I]Pysy kanavalla,
            //[I]koska myöhemmin tänään

            const string paragraphWriteFormat = "[{4}]{3}{0}{3}{1}{3}{2}";
            var sb = new StringBuilder();
            sb.AppendLine(@"[HEADER]
SUBTITLING_COMPANY=Softitler Net, Inc.
TIME_FORMAT=NTSC
CLIENT=UNIVERSAL
LANGUAGE=Finnish
DATE=5/28/2007
JOB_ID=89972
JOB_TYPE=Feature
TITLE=Notting Hill
SUBNAME=BD TRTL
YEAR=1999
DIGITAL_CINEMA=YES
[/HEADER]
[BODY]");
            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                count++;
                string text = p.Text.Replace("<i>", "[I]").Replace("</i>", "[/I]");
                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text, Environment.NewLine, count));
            }
            sb.AppendLine("[/BODY]");
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            ExpectingLine expecting = ExpectingLine.Number;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (RegexNumber.IsMatch(line))
                {
                    if (paragraph != null)
                        subtitle.Paragraphs.Add(paragraph);
                    paragraph = new Paragraph();
                    expecting = ExpectingLine.TimeStart;
                }
                else if (paragraph != null && expecting == ExpectingLine.TimeStart && RegexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            var tc = DecodeTimeCode(parts);
                            paragraph.StartTime = tc;
                            expecting = ExpectingLine.TimeEnd;
                        }
                        catch
                        {
                            _errorCount++;
                            expecting = ExpectingLine.Number;
                        }
                    }
                }
                else if (paragraph != null && expecting == ExpectingLine.TimeEnd && RegexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            var tc = DecodeTimeCode(parts);
                            paragraph.EndTime = tc;
                            expecting = ExpectingLine.Text;
                        }
                        catch
                        {
                            _errorCount++;
                            expecting = ExpectingLine.Number;
                        }
                    }
                }
                else
                {
                    if (paragraph != null && expecting == ExpectingLine.Text)
                    {
                        if (line.Length > 0)
                        {
                            string s = line;
                            if (line.ToLower().StartsWith("[i]"))
                            {
                                s = "<i>" + s.Remove(0, 3);
                                if (s.ToLower().EndsWith("[/i]"))
                                    s = s.Remove(s.Length - 4, 4);
                                s += "</i>";
                            }
                            s = s.Replace("[I]", "<i>");
                            s = s.Replace("[/I]", "</i>");
                            s = s.Replace("[P]", string.Empty);
                            s = s.Replace("[/P]", string.Empty);
                            paragraph.Text = (paragraph.Text + Environment.NewLine + s).Trim();
                            if (paragraph.Text.Length > 2000)
                            {
                                _errorCount += 100;
                                return;
                            }
                        }
                    }
                }
            }
            subtitle.Renumber(1);
        }

        private string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFrames(time.Milliseconds));
        }

        private TimeCode DecodeTimeCode(string[] parts)
        {
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            var milliseconds = (int)((1000.0 / Configuration.Settings.General.CurrentFrameRate) * int.Parse(frames));
            if (milliseconds > 999)
                milliseconds = 999;

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), milliseconds);
        }

    }
}