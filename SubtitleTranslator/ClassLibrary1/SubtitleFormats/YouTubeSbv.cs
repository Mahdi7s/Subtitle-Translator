﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class YouTubeSbv : SubtitleFormat
    {
        enum ExpectingLine
        {
            TimeCodes,
            Text
        }

        Paragraph _paragraph;
        ExpectingLine _expecting = ExpectingLine.TimeCodes;
        static readonly Regex RegexTimeCodes = new Regex(@"^-?\d+:-?\d+:-?\d+[:,.]-?\d+,\d+:-?\d+:-?\d+[:,.]-?\d+$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".sbv"; }
        }

        public override string Name
        {
            get { return "Youtube sbv"; }
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
            const string paragraphWriteFormat = "{0},{1}\r\n{2}\r\n\r\n";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.Append(string.Format(paragraphWriteFormat, FormatTime(p.StartTime), FormatTime(p.EndTime), p.Text));
            }
            return sb.ToString().Trim();
        }

        private string FormatTime(TimeCode timeCode)
        {
            return string.Format("{0}:{1:00}:{2:00}.{3:000}", timeCode.Hours, timeCode.Minutes, timeCode.Seconds, timeCode.Milliseconds);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
//0:00:07.500,0:00:13.500
//In den Bergen über Musanze in Ruanda feiert die Trustbank (Kreditnehmer-Gruppe)  "Trususanze" ihren Erfolg.

//0:00:14.000,0:00:17.000
//Indem sie ihre Zukunft einander anvertraut haben, haben sie sich

            _paragraph = new Paragraph();
            _expecting = ExpectingLine.TimeCodes;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].TrimEnd();
                string next = string.Empty;
                if (i + 1 < lines.Count)
                    next = lines[i + 1];

                // A new line is missing between two paragraphs (buggy srt file)
                if (_expecting == ExpectingLine.Text && i + 1 < lines.Count &&
                    _paragraph != null && !string.IsNullOrEmpty(_paragraph.Text) &&
                    RegexTimeCodes.IsMatch(lines[i]))
                {
                    ReadLine(subtitle, string.Empty, string.Empty);
                }

                ReadLine(subtitle, line, next);
            }
            if (_paragraph.Text.Trim().Length > 0)
                subtitle.Paragraphs.Add(_paragraph);

            foreach (Paragraph p in subtitle.Paragraphs)
                p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

            subtitle.Renumber(1);
        }

        private void ReadLine(Subtitle subtitle, string line, string next)
        {
            switch (_expecting)
            {
                case ExpectingLine.TimeCodes:
                    if (TryReadTimeCodesLine(line, _paragraph))
                    {
                        _paragraph.Text = string.Empty;
                        _expecting = ExpectingLine.Text;
                    }
                    else if (line.Trim().Length > 0)
                    {
                        _errorCount++;
                        _expecting = ExpectingLine.TimeCodes; // lets go to next paragraph
                    }
                    break;
                case ExpectingLine.Text:
                    if (line.Trim().Length > 0)
                    {
                        if (_paragraph.Text.Length > 0)
                            _paragraph.Text += Environment.NewLine;
                        _paragraph.Text += RemoveBadChars(line).TrimEnd();
                    }
                    else if (IsText(next))
                    {
                        if (_paragraph.Text.Length > 0)
                            _paragraph.Text += Environment.NewLine;
                        _paragraph.Text += RemoveBadChars(line).TrimEnd();
                    }
                    else
                    {
                        subtitle.Paragraphs.Add(_paragraph);
                        _paragraph = new Paragraph();
                        _expecting = ExpectingLine.TimeCodes;
                    }
                    break;
            }
        }

        private bool IsText(string text)
        {
            if (text.Trim().Length == 0)
                return false;

            if (Utilities.IsInteger(text))
                return false;

            if (RegexTimeCodes.IsMatch(text))
                return false;

            return true;
        }

        private string RemoveBadChars(string line)
        {
            line = line.Replace("\0", " ");

            return line;
        }

        private bool TryReadTimeCodesLine(string line, Paragraph paragraph)
        {
            line = line.Replace(".", ":");
            line = line.Replace("،", ",");
            line = line.Replace("¡", ":");

            if (RegexTimeCodes.IsMatch(line))
            {
                line = line.Replace(",", ":");
                string[] parts = line.Replace(" ", string.Empty).Split(':', ',');
                try
                {
                    int startHours = int.Parse(parts[0]);
                    int startMinutes = int.Parse(parts[1]);
                    int startSeconds = int.Parse(parts[2]);
                    int startMilliseconds = int.Parse(parts[3]);
                    int endHours = int.Parse(parts[4]);
                    int endMinutes = int.Parse(parts[5]);
                    int endSeconds = int.Parse(parts[6]);
                    int endMilliseconds = int.Parse(parts[7]);
                    paragraph.StartTime = new TimeCode(startHours, startMinutes, startSeconds, startMilliseconds);
                    paragraph.EndTime = new TimeCode(endHours, endMinutes, endSeconds, endMilliseconds);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }
}
