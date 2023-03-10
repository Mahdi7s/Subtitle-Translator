using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class UnknownSubtitle9 : SubtitleFormat
    {
        //00:04:04.219
        //The city council of long beach

        public override string Extension
        {
            get { return ".html"; }
        }

        public override string Name
        {
            get { return "Unknown9"; }
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
            var sb = new StringBuilder();
            sb.AppendLine("<div id=\"transcript\">");
            sb.AppendLine("  <div name=\"transcriptText\" id=\"transcriptText\">");
            sb.AppendLine("    <div id=\"transcriptPanel\">");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format("      <a class=\"caption\" starttime=\"{0}\" duration=\"{1}\">{2}</a>", p.StartTime.TotalMilliseconds, p.Duration.TotalMilliseconds, p.Text.Replace(Environment.NewLine, "<br />")));
            }
            sb.AppendLine("    </div>");
            sb.AppendLine("  </div>");
            sb.AppendLine("</div>");
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //<a class="caption" starttime="0" duration="16000">[♪techno music♪]</a>

            StringBuilder temp = new StringBuilder();
            foreach (string l in lines)
                temp.Append(l);
            string all = temp.ToString();
            if (!all.Contains("class=\"caption\""))
                return;

            Paragraph paragraph = new Paragraph();
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                int indexOfStart = line.IndexOf("starttime=");
                int indexOfDuration = line.IndexOf("duration=");
                if (line.Contains("class=\"caption\"") && indexOfStart > 0 && indexOfDuration > 0)
                {
                    string startTime = "0";
                    int index = indexOfStart +10;
                    while (index < line.Length && "0123456789\"'.,".Contains(line[index].ToString()))
                    {
                        if ("0123456789,.".Contains(line[index].ToString()))
                            startTime += line[index].ToString();
                        index++;
                    }

                    string duration = "0";
                    index = indexOfDuration + 9;
                    while (index < line.Length && "0123456789\"'.,".Contains(line[index].ToString()))
                    {
                        if ("0123456789,.".Contains(line[index].ToString()))
                            duration += line[index].ToString();
                        index++;
                    }

                    string text = string.Empty;
                    index = line.IndexOf(">", indexOfDuration);
                    if (index > 0 && index+ 1 < line.Length)
                    {
                        text = line.Substring(index + 1).Trim();
                        index = text.IndexOf("</");
                        if (index > 0)
                            text = text.Substring(0, index);
                        text = text.Replace("<br />", Environment.NewLine);
                    }

                    int startMilliseconds;
                    int durationMilliseconds;
                    if (text.Length > 0 && int.TryParse(startTime, out startMilliseconds) && int.TryParse(duration, out durationMilliseconds))
                        subtitle.Paragraphs.Add(new Paragraph(text, startMilliseconds, startMilliseconds + durationMilliseconds));

                }
            }
            subtitle.Renumber(1);
        }

    }
}

