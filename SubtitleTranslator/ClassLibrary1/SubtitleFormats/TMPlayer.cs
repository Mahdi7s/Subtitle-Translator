﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{

    public class TMPlayer : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "TMPlayer"; }
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

            if (subtitle.Paragraphs.Count > 4)
            {
                bool allStartWithNumber = true;
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    if (p.Text.Length > 1  && !Utilities.IsInteger(p.Text.Substring(0, 2)))
                    {
                        allStartWithNumber = false;
                        break;
                    }
                }
                if (allStartWithNumber)
                    return false;
            }
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = Utilities.RemoveHtmlTags(p.Text);
                text = text.Replace(Environment.NewLine, "|");
                sb.AppendLine(string.Format("{0:00}:{1:00}:{2:00}:{3}", p.StartTime.Hours,
                                                               p.StartTime.Minutes,
                                                               p.StartTime.Seconds,
                                                               text));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        { // 0:02:36:You've returned to the village|after 2 years, Shekhar.
          // 00:00:50:America has made my fortune.
            var regex = new Regex(@"^\d+:\d\d:\d\d[: ].*$", RegexOptions.Compiled); // accept a " " instead of the last ":" too
            _errorCount = 0;
            foreach (string line in lines)
            {
                bool success = false;
                if (regex.Match(line).Success)
                {
                    string s = line;
                    if (line.Length > 9 && line[8] == ' ')
                        s = line.Substring(0, 8) + ":" + line.Substring(9);
                    string[] parts = s.Split(':');
                    if (parts.Length > 3)
                    {
                        int hours = int.Parse(parts[0]);
                        int minutes = int.Parse(parts[1]);
                        int seconds = int.Parse(parts[2]);
                        string text = string.Empty;
                        for (int i = 3; i < parts.Length; i++ )
                        {
                            if (text.Length == 0)
                                text = parts[i];
                            else
                                text += ":" + parts[i];
                        }
                        text = text.Replace("|", Environment.NewLine);
                        var start = new TimeCode(hours, minutes, seconds, 0);
                        double duration = Utilities.GetDisplayMillisecondsFromText(text) * 1.2;
                        var end = new TimeCode(TimeSpan.FromMilliseconds(start.TotalMilliseconds + duration));

                        var p = new Paragraph(start, end, text);
                        subtitle.Paragraphs.Add(p);
                        success = true;
                    }
                }
                if (!success)
                    _errorCount++;
            }

            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                Paragraph next = subtitle.GetParagraphOrDefault(index+1);
                if (next != null && next.StartTime.TotalMilliseconds <= p.EndTime.TotalMilliseconds)
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;

                index++;
                p.Number = index;
            }

        }
    }
}
