﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class FinalCutProXXml : SubtitleFormat
    {
        public double FrameRate { get; set; }

        public override string Extension
        {
            get { return ".fcpxml"; }
        }

        public override string Name
        {
            get { return "Final Cut Pro X Xml"; }
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
            return subtitle.Paragraphs.Count > 0;
        }

        private string IsNtsc()
        {
            if (FrameRate >= 29.976 && FrameRate <= 30.0)
                return "TRUE";
            if (FrameRate < 29.976)
                return "FALSE";
            return "TRUE";
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            if (Configuration.Settings.General.CurrentFrameRate > 26)
                FrameRate = 30;
            else
                FrameRate = 25;

            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>" + Environment.NewLine +
                "<!DOCTYPE fcpxml>" + Environment.NewLine +
                Environment.NewLine +
                "<fcpxml version=\"1.1\">" + Environment.NewLine +
                "  <project name=\"Yma LIVE in Moscow\" uid=\"C1E80D31-57D4-4E6C-84F6-86A75DCB7A54\" eventID=\"B5C98F73-1D7E-4205-AEF3-1485842EB191\" location=\"file://localhost/Volumes/Macintosh%20HD/Final%20Cut%20Projects/Yma%20Sumac/Yma%20LIVE%20in%20Moscow/\" >" + Environment.NewLine +
                "    <sequence duration=\"10282752480/2400000s\" format=\"r1\" tcStart=\"0s\" tcFormat=\"NDF\" audioLayout=\"stereo\" audioRate=\"48k\">" + Environment.NewLine +
                "      <spine>" + Environment.NewLine +
                "      </spine>" + Environment.NewLine +
                "    </sequence>" + Environment.NewLine +
                "  </project>" + Environment.NewLine +
                "</fcpxml>";

            string xmlClipStructure =
                "  <title lane=\"6\" offset=\"4130126/60000s\" ref=\"r6\" name=\"\" duration=\"288288/60000s\" start=\"216003788/60000s\" role=\"Subtitles\">" + Environment.NewLine +
                "    <adjust-transform position=\"0.267518 -32.3158\"/>" + Environment.NewLine +
                "    <text></text>" + Environment.NewLine +
                "  </title>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            XmlNode videoNode = xml.DocumentElement.SelectSingleNode("project/sequence/spine");

            int number = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode clip = xml.CreateElement("clip");
                clip.InnerXml = xmlClipStructure;
                var attr = xml.CreateAttribute("name");
                attr.Value = title;
                clip.Attributes.Append(attr);

                attr = xml.CreateAttribute("duration");
                //attr.Value = "9529520/2400000s";
                attr.Value = Convert.ToInt64(p.Duration.TotalSeconds * 2400000) + "/2400000s";
                clip.Attributes.Append(attr);

                attr = xml.CreateAttribute("start");
                //attr.Value = "1201200/2400000s";
                attr.Value = Convert.ToInt64(p.StartTime.TotalSeconds * 2400000) + "/2400000s";
                clip.Attributes.Append(attr);

                attr = xml.CreateAttribute("audioStart");
                attr.Value = "0s";
                clip.Attributes.Append(attr);

                attr = xml.CreateAttribute("audioDuration");
                attr.Value = Convert.ToInt64(p.Duration.TotalSeconds * 2400000) + "/2400000s";
                clip.Attributes.Append(attr);

                attr = xml.CreateAttribute("tcFormat");
                attr.Value = "NDF";
                clip.Attributes.Append(attr);

                XmlNode titleNode = clip.SelectSingleNode("title");
                titleNode.Attributes["offset"].Value = Convert.ToInt64(p.StartTime.TotalSeconds * 60000) + "/60000s";
                titleNode.Attributes["name"].Value = Utilities.RemoveHtmlTags(p.Text);
                titleNode.Attributes["duration"].Value = Convert.ToInt64(p.Duration.TotalSeconds * 60000) + "/60000s";
                titleNode.Attributes["start"].Value = Convert.ToInt64(p.StartTime.TotalSeconds * 60000) + "/60000s";

                XmlNode text = clip.SelectSingleNode("title/text");
                text.InnerText = Utilities.RemoveHtmlTags(p.Text);

                videoNode.AppendChild(clip);
                number++;
            }

            var ms = new MemoryStream();
            var writer = new XmlTextWriter(ms, Encoding.UTF8) {Formatting = Formatting.Indented};
            xml.Save(writer);
            string xmlAsText = Encoding.UTF8.GetString(ms.ToArray()).Trim();
            xmlAsText = xmlAsText.Replace("fcpxml[]", "fcpxml");
            return xmlAsText;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            FrameRate = Configuration.Settings.General.CurrentFrameRate;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(sb.ToString());

                foreach (XmlNode node in xml.SelectNodes("fcpxml/project/sequence/spine/clip"))
                {
                    try
                    {
                        foreach (XmlNode title in node.SelectNodes("title"))
                        {
                            var role = title.Attributes["role"];
                            if (role != null && role.InnerText == "Subtitles")
                            {
                                var textNode = title.SelectSingleNode("text");
                                if (textNode != null && !string.IsNullOrEmpty(textNode.InnerText))
                                {
                                    string text = textNode.InnerText;
                                    Paragraph p = new Paragraph();
                                    p.Text = text.Trim();
                                    p.StartTime = DecodeTime(title.Attributes["offset"]);
                                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + DecodeTime(title.Attributes["duration"]).TotalMilliseconds;                                    
                                    subtitle.Paragraphs.Add(p);                                    
                                }
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
            catch
            {
                _errorCount = 1;
                return;
            }

        }

        private TimeCode DecodeTime(XmlAttribute duration)
        {
            // 220220/60000s
            if (duration != null)
            {

                var arr = duration.Value.TrimEnd('s').Split('/');
                if (arr.Length == 2)
                {
                    return new TimeCode(TimeSpan.FromSeconds(long.Parse(arr[0]) / double.Parse(arr[1]) ));
                }
                else if (arr.Length == 1)
                {
                    return new TimeCode(TimeSpan.FromSeconds(float.Parse(arr[0])));
                }
            }
            return new TimeCode(0, 0, 0, 0);
        }

    }
}


