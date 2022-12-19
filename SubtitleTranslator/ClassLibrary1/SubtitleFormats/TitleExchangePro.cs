﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class TitleExchangePro : SubtitleFormat
    {
        static Regex regexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d,\t[ ]?\d\d:\d\d:\d\d:\d\d,\t[ ]?", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "TitleExchange Pro"; }
        }

        public override bool HasLineNumber
        {
            get { return true; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();

            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);
            if (sb.ToString().Contains(Environment.NewLine + "SP_NUMBER\tSTART\tEND\tFILE_NAME"))
                return false; // SON

            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            StringBuilder sb = new StringBuilder();
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                index++;
                //00:01:48:22,       00:01:52:17,       - I need those samples, fast!//- Yes, professor.
                string text = p.Text;
                text = text.Replace("<i>", "@Italic@");
                text = text.Replace("</i>", "@Italic@");
                text = text.Replace(Environment.NewLine, "//");
                sb.AppendLine(string.Format("{1},\t{2},\t{3}", index, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), Utilities.RemoveHtmlTags(text)));
            }
            return sb.ToString();
        }

        private string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFrames(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:01:48:22,       00:01:52:17,       - I need those samples, fast!//- Yes, professor.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                string s = line.Replace("       ", "\t");
                if (regexTimeCodes.IsMatch(s))
                {
                    var temp = s.Split('\t');
                    if (temp.Length > 1)
                    {
                        string start = temp[0].TrimEnd(',');
                        string end = temp[1].TrimEnd(',');

                        string[] startParts = start.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string[] endParts = end.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (startParts.Length == 4 && endParts.Length == 4)
                        {
                            try
                            {
                                string text = s.Remove(0, regexTimeCodes.Match(s).Length - 1).Trim();
                                if (!text.Contains(Environment.NewLine))
                                    text = text.Replace("//", Environment.NewLine);
                                if (text.Contains("@Italic@"))
                                {
                                    bool italicOn = false;
                                    while (text.Contains("@Italic@"))
                                    {
                                        int index = text.IndexOf("@Italic@");
                                        string italicTag = "<i>";
                                        if (italicOn)
                                            italicTag = "</i>";
                                        text = text.Remove(index, "@Italic@".Length).Insert(index, italicTag);
                                        italicOn = !italicOn;
                                    }
                                    text = Utilities.FixInvalidItalicTags(text);
                                }
                                p = new Paragraph(DecodeTimeCode(startParts), DecodeTimeCode(endParts), text);
                                subtitle.Paragraphs.Add(p);
                            }
                            catch (Exception exception)
                            {
                                _errorCount++;
                                System.Diagnostics.Debug.WriteLine(exception.Message);
                            }
                        }
                    }
                }
                else if (line.Trim().Length == 0)
                {
                    // skip empty lines
                }
                else if (line.Trim().Length > 0 && p != null)
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber(1);
        }

        private TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            int milliseconds = (int)((1000.0 / Configuration.Settings.General.CurrentFrameRate) * int.Parse(frames));
            if (milliseconds > 999)
                milliseconds = 999;

            TimeCode tc = new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), milliseconds);
            return tc;
        }

    }
}

