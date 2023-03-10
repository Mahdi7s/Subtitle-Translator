using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class AQTitle : SubtitleFormat
    {

        enum ExpectingLine
        {
            TimeStart,
            Text,
            TimeEndOrText,
        }

        public override string Extension
        {
            get { return ".aqt"; }
        }

        public override string Name
        {
            get { return "AQTitle"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
        }

        public override bool IsTimeBased
        {
            get { return false; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            Subtitle subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
//-->> 072058
//<i>Meine Mutter und meine Schwester,

//-->> 072169


//-->> 072172
//<i>die in Zürich lebt, und ich,

//-->> 072247
            const string paragraphWriteFormat = "-->> {0}{3}{2}{3}-->> {1}{3}{3}";

            StringBuilder sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = p.Text;
                int newLines = Utilities.CountTagInText(text, Environment.NewLine);
                if (newLines > 1)
                    text = Utilities.AutoBreakLine(text);
                else if (newLines == 0)
                    text += Environment.NewLine;

                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text, Environment.NewLine));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var paragraph = new Paragraph();
            ExpectingLine expecting = ExpectingLine.TimeStart;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (line.StartsWith("-->> "))
                {
                    string timePart = line.Substring(4).Trim();
                    if (timePart.Length > 0)
                    {
                        try
                        {
                            var tc = DecodeTimeCode(timePart);
                            if (expecting == ExpectingLine.TimeStart)
                            {
                                paragraph = new Paragraph();
                                paragraph.StartFrame = int.Parse(timePart);
                                paragraph.StartTime = tc;
                                expecting = ExpectingLine.Text;
                            }
                            else if (expecting == ExpectingLine.TimeEndOrText)
                            {
                                paragraph.EndFrame = int.Parse(timePart);
                                paragraph.EndTime = tc;
                                subtitle.Paragraphs.Add(paragraph);
                                paragraph = new Paragraph();
                                expecting = ExpectingLine.TimeStart;
                            }
                        }
                        catch
                        {
                            _errorCount++;
                            expecting = ExpectingLine.TimeStart;
                        }
                    }
                }
                else
                {
                    if (expecting == ExpectingLine.Text || expecting == ExpectingLine.TimeEndOrText)
                    {
                        if (line.Length > 0)
                        {
                            string text = line.Replace("|", Environment.NewLine);
                            if (string.IsNullOrEmpty(paragraph.Text))
                                paragraph.Text = text.Trim();
                            else
                                paragraph.Text += Environment.NewLine + text;
                            if (paragraph.Text.Length > 2000)
                            {
                                _errorCount += 100;
                                return;
                            }
                        }
                        expecting = ExpectingLine.TimeEndOrText;
                    }
                    else if (expecting == ExpectingLine.TimeStart && line.Trim().Length > 0 && paragraph != null)
                    {
                        int ms = (int)paragraph.EndTime.TotalMilliseconds;
                        int frames = paragraph.EndFrame;
                        paragraph = new Paragraph();
                        paragraph.StartTime.TotalMilliseconds = ms;
                        paragraph.StartFrame = frames;
                        paragraph.Text = line.Trim();
                        expecting = ExpectingLine.TimeEndOrText;
                    }
                }
            }
            subtitle.Renumber(1);

        }

        private string EncodeTimeCode(TimeCode time)
        {
            int frames = MillisecondsToFrames(time.TotalMilliseconds) + 1;
            return frames.ToString();
        }

        private TimeCode DecodeTimeCode(string timePart)
        {
            int milliseconds = (int)((1000.0 / Configuration.Settings.General.CurrentFrameRate) * int.Parse(timePart));
            TimeSpan ts = TimeSpan.FromMilliseconds(milliseconds);
            return new TimeCode(ts);
        }

    }
}