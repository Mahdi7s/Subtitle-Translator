﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class CaraokeXml : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".crk"; }
        }

        public override string Name
        {
            get { return "Caraoke Xml"; }
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

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<caraoke name=\"\" filename=\"\"><paragraph attr=\"\" /></caraoke>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            var paragraph = xml.DocumentElement.SelectSingleNode("paragraph");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode item = xml.CreateElement("item");

                var start = xml.CreateAttribute("tc1");
                start.InnerText = p.StartTime.TotalMilliseconds.ToString();
                item.Attributes.Append(start);

                var end = xml.CreateAttribute("tc2");
                end.InnerText = p.EndTime.TotalMilliseconds.ToString();
                item.Attributes.Append(end);

                var attr = xml.CreateAttribute("attr");
                attr.InnerText = string.Empty;
                item.Attributes.Append(attr);

                item.InnerText = p.Text;

                paragraph.AppendChild(item);
            }

            var ms = new MemoryStream();
            var writer = new XmlTextWriter(ms, Encoding.UTF8) {Formatting = Formatting.Indented};
            xml.Save(writer);
            return Encoding.UTF8.GetString(ms.ToArray()).Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string xmlAsText = sb.ToString();

            if (!xmlAsText.Contains("<caraoke"))
                return;

            xmlAsText = xmlAsText.Replace("< /", "</");

            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(xmlAsText);
            }
            catch (Exception ex )
            {
                _errorCount = 1;
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return;
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//item"))
            {
                try
                {
                    string start = node.Attributes["tc1"].InnerText;
                    string end = node.Attributes["tc2"].InnerText;
                    string text = node.InnerText;

                    subtitle.Paragraphs.Add(new Paragraph(text, Convert.ToDouble(start), Convert.ToDouble(end)));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber(1);
        }

    }
}


