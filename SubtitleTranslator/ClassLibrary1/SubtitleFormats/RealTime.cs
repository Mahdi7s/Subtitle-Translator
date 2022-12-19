﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class RealTime : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".rt"; }
        }

        public override string Name
        {
            get { return "RealTime"; }
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
            StringBuilder sb = new StringBuilder();
            int index = 0;
            sb.AppendLine("<Window" + Environment.NewLine +
                "  Width    = \"640\"" + Environment.NewLine +
                "  Height   = \"480\"" + Environment.NewLine +
                "  WordWrap = \"true\"" + Environment.NewLine +
                "  Loop     = \"true\"" + Environment.NewLine +
                "  bgcolor  = \"black\"" + Environment.NewLine +
                ">" + Environment.NewLine +
                "<Font" + Environment.NewLine +
                "  Color = \"white\"" + Environment.NewLine +
                "  Face  = \"Arial\"" + Environment.NewLine +
                "  Size  = \"+2\"" + Environment.NewLine +
                ">" + Environment.NewLine +
                "<center>" + Environment.NewLine +
                "<b>" + Environment.NewLine);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //<Time begin="0:03:24.8" end="0:03:29.4" /><clear/>Man stjæler ikke fra Chavo, nej.
                sb.AppendLine(string.Format("<Time begin=\"{0}\" end=\"{1}\" /><clear/>{2}", EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), p.Text.Replace(Environment.NewLine, " ")));
                index++;
            }
            sb.AppendLine("</b>");
            sb.AppendLine("</center>");
            return sb.ToString();
        }

        private string EncodeTimeCode(TimeCode time)
        {
            //0:03:24.8
            return string.Format("{0:0}:{1:00}:{2:00}.{3:0}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds / 100);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //<Time begin="0:03:24.8" end="0:03:29.4" /><clear/>Man stjæler ikke fra Chavo, nej.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                try
                {
                    if (line.Contains("<Time ") && line.Contains(" begin=") && line.Contains("end="))
                    {
                        int indexOfBegin = line.IndexOf(" begin=");
                        int indexOfEnd = line.IndexOf(" end=");
                        string begin = line.Substring(indexOfBegin + 7, 11);
                        string end = line.Substring(indexOfEnd + 5, 11);

                            string[] startParts = begin.Split(":.\"".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            string[] endParts = end.Split(":.\"".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            if (startParts.Length == 4 && endParts.Length == 4)
                            {
                                string text = line.Substring(line.LastIndexOf("/>") + 2);
                                p = new Paragraph(DecodeTimeCode(startParts), DecodeTimeCode(endParts), text);
                                subtitle.Paragraphs.Add(p);
                            }
                    }
                }
                catch
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber(1);
        }

        private TimeCode DecodeTimeCode(string[] parts)
        {
            //[00:06:51.48]
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string millisesonds = parts[3];

            TimeCode tc = new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), int.Parse(millisesonds) * 10);
            return tc;
        }

    }
}

