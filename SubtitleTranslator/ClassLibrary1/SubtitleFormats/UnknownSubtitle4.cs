using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class UnknownSubtitle4 : SubtitleFormat
    {
        enum ExpectingLine
        {
            TimeCodes,
            Text
        }

        public override string Extension
        {
            get { return ".sub"; }
        }

        public override string Name
        {
            get { return "Unknown 4"; }
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
            const string paragraphWriteFormat = "{0:00}:{1:00}:{2:00}.{3:00}, {4:00}:{5:00}:{6:00}.{7:00}{8}{9}";

//00:00:07.00, 00:00:12.00
//Welche Auswirkung Mikroversicherungen auf unsere Klienten hat? Lassen wir sie für sich selber sprechen!
//
//00:00:22.00, 00:00:27.00
//Arme Menschen in Uganda leben oft in schlechten Unterkünften.

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = p.Text.Replace(Environment.NewLine, "|");

                sb.AppendLine(string.Format(paragraphWriteFormat,
                                        p.StartTime.Hours,
                                        p.StartTime.Minutes,
                                        p.StartTime.Seconds,
                                        RoundTo2Cifres(p.StartTime.Milliseconds),
                                        p.EndTime.Hours,
                                        p.EndTime.Minutes,
                                        p.EndTime.Seconds,
                                        RoundTo2Cifres(p.EndTime.Milliseconds),
                                        Environment.NewLine,
                                        text));
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }

        private int RoundTo2Cifres(int milliseconds)
        {
            int rounded = (int)Math.Round((double)milliseconds / 10);
            return rounded;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var regexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d.\d+, \d\d:\d\d:\d\d.\d+$", RegexOptions.Compiled);

            var paragraph = new Paragraph();
            ExpectingLine expecting = ExpectingLine.TimeCodes;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (regexTimeCodes.IsMatch(line))
                {
                    string[] parts = line.Split(new[] { ':', ',', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 8)
                    {
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
                            expecting = ExpectingLine.Text;
                        }
                        catch
                        {
                            expecting = ExpectingLine.TimeCodes;
                        }
                    }
                }
                else
                {
                    if (expecting == ExpectingLine.Text)
                    {
                        if (line.Length > 0)
                        {
                            string text = line.Replace("|", Environment.NewLine);
                            text = line.Replace("[br]", Environment.NewLine);
                            text = line.Replace("<br>", Environment.NewLine);
                            text = line.Replace("<br />", Environment.NewLine);
                            text = text.Replace("{\\i1}", "<i>");
                            text = text.Replace("{\\i0}", "</i>");
                            text = text.Replace("{\\b1}", "<b>'");
                            text = text.Replace("{\\b0}", "</b>");
                            text = text.Replace("{\\u1}", "<u>");
                            text = text.Replace("{\\u0}", "</u>");

                            paragraph.Text = text;
                            subtitle.Paragraphs.Add(paragraph);
                            paragraph = new Paragraph();
                            expecting = ExpectingLine.TimeCodes;
                        }
                    }
                }
            }
            subtitle.Renumber(1);
        }
    }
}
