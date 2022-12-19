﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class CaptionateMs : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Captionate MS"; }
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
            Subtitle subtitle = new Subtitle();
            this.LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > 0;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure = @"<captionate>
<timeformat>ms</timeformat>
<namesareprefixed>namesareprefixed</namesareprefixed>
<captioninfo>
<trackinfo>
<track>
<displayname>Default</displayname>
<type/>
<languagecode/>
<targetwpm>140</targetwpm>
<stringdata/>
</track>
</trackinfo>
<speakerinfo>
</speakerinfo>
</captioninfo>
<captions></captions></captionate>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            Paragraph last = null;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (last != null)
                {
                    if (last.EndTime.TotalMilliseconds + 500 < p.StartTime.TotalMilliseconds)
                    {
                        Paragraph blank = new Paragraph();
                        blank.StartTime.TotalMilliseconds = last.EndTime.TotalMilliseconds;
                        AddParagraph(xml, blank);
                    }
                }

                AddParagraph(xml, p);
                last = p;
            }

            var ms = new MemoryStream();
            var writer = new XmlTextWriter(ms, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            xml.Save(writer);
            return Encoding.UTF8.GetString(ms.ToArray()).Trim();
        }

        private void AddParagraph(XmlDocument xml, Paragraph p)
        {
            XmlNode paragraph = xml.CreateElement("caption");

            XmlAttribute start = xml.CreateAttribute("time");
            start.InnerText = EncodeTime(p.StartTime);
            paragraph.Attributes.Append(start);

            if (p.Text.Trim().Length > 0)
            {
                XmlNode tracks = xml.CreateElement("tracks");
                paragraph.AppendChild(tracks);

                XmlNode track0 = xml.CreateElement("track0");
                track0.InnerText = Utilities.RemoveHtmlTags(p.Text);
                track0.InnerXml = track0.InnerXml.Replace(Environment.NewLine, "<br />");
                tracks.AppendChild(track0);
            }
            xml.DocumentElement.SelectSingleNode("captions").AppendChild(paragraph);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string xmlString = sb.ToString();
            if (!xmlString.Contains("<captionate>") || !xmlString.Contains("</caption>"))
                return;

            XmlDocument xml = new XmlDocument();
            try
            {
                xml.LoadXml(xmlString);
            }
            catch
            {
                _errorCount = 1;
                return;
            }

            Paragraph p = null;
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("captions/caption"))
            {
                try
                {
                    if (node.Attributes["time"] != null)
                    {
                        string start = node.Attributes["time"].InnerText;
                        double startMilliseconds = double.Parse(start);
                        if (p != null)
                            p.EndTime.TotalMilliseconds = startMilliseconds - 1;
                        if (node.SelectSingleNode("tracks/track0") != null)
                        {
                            string text = node.SelectSingleNode("tracks/track0").InnerText;
                            text = Utilities.RemoveHtmlTags(text);
                            text = text.Replace("<br>", Environment.NewLine).Replace("<br />", Environment.NewLine).Replace("<BR>", Environment.NewLine);
                            p = new Paragraph(text, startMilliseconds, startMilliseconds + 3000);
                            if (text.Trim().Length > 0)
                                subtitle.Paragraphs.Add(p);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber(1);
        }

        private string EncodeTime(TimeCode time)
        {
            return time.TotalMilliseconds.ToString();
        }

    }
}


