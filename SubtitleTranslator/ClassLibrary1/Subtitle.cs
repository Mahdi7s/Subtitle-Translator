﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Enums;

namespace Nikse.SubtitleEdit.Logic
{
    public class Subtitle
    {
        List<Paragraph> _paragraphs;
        List<HistoryItem> _history;
        SubtitleFormat _format;
        bool _wasLoadedWithFrameNumbers;
        public string Header { get; set; }
        public string Footer { get; set; }

        public string FileName { get; set; }

        public const int MaximumHistoryItems = 100;

        public SubtitleFormat OriginalFormat
        {
            get
            {
                return _format;
            }
        }

        public List<HistoryItem> HistoryItems
        {
            get { return _history; }
        }

        public Subtitle()
        {
            _paragraphs = new List<Paragraph>();
            _history = new List<HistoryItem>();
            FileName = "Untitled";
        }

        public Subtitle(List<HistoryItem> historyItems) : this()
        {
            _history = historyItems;
        }

        /// <summary>
        /// Copy constructor (only paragraphs)
        /// </summary>
        /// <param name="subtitle">Subtitle to copy</param>
        public Subtitle(Subtitle subtitle) : this()
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                _paragraphs.Add(new Paragraph(p));
            }
            _wasLoadedWithFrameNumbers = subtitle.WasLoadedWithFrameNumbers;
        }

        public List<Paragraph> Paragraphs
        {
            get
            {
                return _paragraphs;
            }
        }

        public Paragraph GetParagraphOrDefault(int index)
        {
            if (_paragraphs == null || _paragraphs.Count <= index || index < 0)
                return null;

            return _paragraphs[index];
        }

        public SubtitleFormat ReloadLoadSubtitle(List<string> lines, string fileName)
        {
            Paragraphs.Clear();
            foreach (SubtitleFormat subtitleFormat in SubtitleFormat.AllSubtitleFormats)
            {
                if (subtitleFormat.IsMine(lines, fileName))
                {
                    subtitleFormat.LoadSubtitle(this, lines, fileName);
                    _format = subtitleFormat;
                    return subtitleFormat;
                }
            }
            return null;
        }

        public SubtitleFormat LoadSubtitle(string fileName, out Encoding encoding, Encoding useThisEncoding)
        {
            FileName = fileName;

            _paragraphs = new List<Paragraph>();

            var lines = new List<string>();
            StreamReader sr;
            if (useThisEncoding != null)
                sr = new StreamReader(fileName, useThisEncoding);
            else
                sr = new StreamReader(fileName, Utilities.GetEncodingFromFile(fileName), true);

            encoding = sr.CurrentEncoding;
            while (!sr.EndOfStream)
                lines.Add(sr.ReadLine());
            sr.Close();

            foreach (SubtitleFormat subtitleFormat in SubtitleFormat.AllSubtitleFormats)
            {
                if (subtitleFormat.IsMine(lines, fileName))
                {
                    Header = null;
                    subtitleFormat.LoadSubtitle(this, lines, fileName);
                    _format = subtitleFormat;
                    _wasLoadedWithFrameNumbers = _format.IsFrameBased;
                    return subtitleFormat;
                }
            }

            if (useThisEncoding == null)
                return LoadSubtitle(fileName, out encoding, Encoding.Unicode);

            return null;
        }

        public void MakeHistoryForUndo(string description, SubtitleFormat subtitleFormat, DateTime fileModified, Subtitle original, string originalSubtitleFileName, int lineNumber, int linePosition, int linePositionAlternate)
        {
            // don't fill memory with history - use a max rollback points
            if (_history.Count > MaximumHistoryItems)
                _history.RemoveAt(0);

            _history.Add(new HistoryItem(_history.Count, this, description, FileName, fileModified, subtitleFormat.FriendlyName, original, originalSubtitleFileName, lineNumber, linePosition, linePositionAlternate));
        }

        public bool CanUndo
        {
            get
            {
                return _history.Count > 0;
            }
        }

        public string UndoHistory(int index, out string subtitleFormatFriendlyName, out DateTime fileModified, out Subtitle originalSubtitle, out string originalSubtitleFileName)
        {
            _paragraphs.Clear();
            foreach (Paragraph p in _history[index].Subtitle.Paragraphs)
                _paragraphs.Add(new Paragraph(p));

            subtitleFormatFriendlyName = _history[index].SubtitleFormatFriendlyName;
            FileName = _history[index].FileName;
            fileModified = _history[index].FileModified;
            originalSubtitle = new Subtitle(_history[index].OriginalSubtitle);
            originalSubtitleFileName = _history[index].OriginalSubtitleFileName;

            return FileName;
        }

        internal string ToText(SubtitleFormat format)
        {
            return format.ToText(this, Path.GetFileNameWithoutExtension(FileName));
        }

        public void AddTimeToAllParagraphs(TimeSpan time)
        {
            foreach (Paragraph p in Paragraphs)
            {
                p.StartTime.AddTime(time);
                p.EndTime.AddTime(time);
            }
        }

        /// <summary>
        /// Calculate the time codes from framenumber/framerate
        /// </summary>
        /// <param name="frameRate">Number of frames per second</param>
        /// <returns>True if times could be calculated</returns>
        public bool CalculateTimeCodesFromFrameNumbers(double frameRate)
        {
            if (_format == null)
                return false;

            if (_format.IsTimeBased)
                return false;

            foreach (Paragraph p in Paragraphs)
            {
                p.CalculateTimeCodesFromFrameNumbers(frameRate);
            }
            return true;
        }

        /// <summary>
        /// Calculate the frame numbers from time codes/framerate
        /// </summary>
        /// <param name="frameRate"></param>
        /// <returns></returns>
        public bool CalculateFrameNumbersFromTimeCodes(double frameRate)
        {
            if (_format == null)
                return false;

            if (_format.IsFrameBased)
                return false;

            foreach (Paragraph p in Paragraphs)
            {
                p.CalculateFrameNumbersFromTimeCodes(frameRate);
            }

            FixEqualOrJustOverlappingFrameNumbers();

            return true;
        }

        public void CalculateFrameNumbersFromTimeCodesNoCheck(double frameRate)
        {
            foreach (Paragraph p in Paragraphs)
                p.CalculateFrameNumbersFromTimeCodes(frameRate);

            FixEqualOrJustOverlappingFrameNumbers();
        }

        private void FixEqualOrJustOverlappingFrameNumbers()
        {
            for (int i = 0; i < Paragraphs.Count - 1; i++)
            {
                Paragraph p = Paragraphs[i];
                Paragraph next = Paragraphs[i + 1];
                if (next != null && p.EndFrame == next.StartFrame || p.EndFrame == next.StartFrame + 1)
                    p.EndFrame = next.StartFrame - 1;
            }
        }

        internal void ChangeFramerate(double oldFramerate, double newFramerate)
        {
            foreach (Paragraph p in Paragraphs)
            {
                p.CalculateFrameNumbersFromTimeCodes(oldFramerate);
                p.CalculateTimeCodesFromFrameNumbers(newFramerate);
            }
        }

        public bool WasLoadedWithFrameNumbers
        {
            get
            {
                return _wasLoadedWithFrameNumbers;
            }
            set
            {
                _wasLoadedWithFrameNumbers = value;
            }
        }

        internal void AdjustDisplayTimeUsingPercent(double percent, System.Windows.Forms.ListView.SelectedIndexCollection selectedIndexes)
        {
            for (int i = 0; i < _paragraphs.Count; i++)
            {
                if (selectedIndexes == null || selectedIndexes.Contains(i))
                {
                    double nextStartMilliseconds = _paragraphs[_paragraphs.Count - 1].EndTime.TotalMilliseconds + 100000;
                    if (i + 1 < _paragraphs.Count)
                        nextStartMilliseconds = _paragraphs[i + 1].StartTime.TotalMilliseconds;

                    double newEndMilliseconds = _paragraphs[i].EndTime.TotalMilliseconds;
                    newEndMilliseconds = _paragraphs[i].StartTime.TotalMilliseconds + (((newEndMilliseconds - _paragraphs[i].StartTime.TotalMilliseconds) * percent) / 100);
                    if (newEndMilliseconds > nextStartMilliseconds)
                        newEndMilliseconds = nextStartMilliseconds - 1;
                    _paragraphs[i].EndTime.TotalMilliseconds = newEndMilliseconds;
                }
            }
        }

        internal void AdjustDisplayTimeUsingSeconds(double seconds, System.Windows.Forms.ListView.SelectedIndexCollection selectedIndexes)
        {
            for (int i = 0; i < _paragraphs.Count; i++)
            {
                if (selectedIndexes == null || selectedIndexes.Contains(i))
                {
                    double nextStartMilliseconds = _paragraphs[_paragraphs.Count - 1].EndTime.TotalMilliseconds + 100000;
                    if (i + 1 < _paragraphs.Count)
                        nextStartMilliseconds = _paragraphs[i + 1].StartTime.TotalMilliseconds;

                    double newEndMilliseconds = _paragraphs[i].EndTime.TotalMilliseconds + (seconds * 1000.0);
                    if (newEndMilliseconds > nextStartMilliseconds)
                        newEndMilliseconds = nextStartMilliseconds - 1;

                    if (seconds < 0)
                    {
                        if (_paragraphs[i].StartTime.TotalMilliseconds + 100 > newEndMilliseconds)
                            _paragraphs[i].EndTime.TotalMilliseconds = _paragraphs[i].StartTime.TotalMilliseconds + 100;
                        else
                            _paragraphs[i].EndTime.TotalMilliseconds = newEndMilliseconds;
                    }
                    else
                    {
                        _paragraphs[i].EndTime.TotalMilliseconds = newEndMilliseconds;
                    }
                }
            }
        }

        internal void Renumber(int startNumber)
        {
            int i = startNumber;
            foreach (Paragraph p in _paragraphs)
            {
                p.Number = i;
                i++;
            }
        }

        internal int GetIndex(Paragraph p)
        {
            if (p == null)
                return -1;

            int index = _paragraphs.IndexOf(p);
            if (index >= 0)
                return index;

            for (int i = 0; i < _paragraphs.Count; i++)
            {
                if (p.StartTime.TotalMilliseconds == _paragraphs[i].StartTime.TotalMilliseconds &&
                    p.EndTime.TotalMilliseconds == _paragraphs[i].EndTime.TotalMilliseconds)
                    return i;
                if (p.Number == _paragraphs[i].Number && (p.StartTime.TotalMilliseconds == _paragraphs[i].StartTime.TotalMilliseconds ||
                    p.EndTime.TotalMilliseconds == _paragraphs[i].EndTime.TotalMilliseconds))
                    return i;
                if (p.Text == _paragraphs[i].Text && (p.StartTime.TotalMilliseconds == _paragraphs[i].StartTime.TotalMilliseconds ||
                    p.EndTime.TotalMilliseconds == _paragraphs[i].EndTime.TotalMilliseconds))
                    return i;
            }
            return -1;
        }

        internal Paragraph GetFirstAlike(Paragraph p)
        {
            foreach (Paragraph item in _paragraphs)
            {
                if (p.StartTime.TotalMilliseconds == item.StartTime.TotalMilliseconds &&
                    p.EndTime.TotalMilliseconds == item.EndTime.TotalMilliseconds &&
                    p.Text == item.Text)
                    return item;
            }
            return null;
        }

        internal Paragraph GetFirstParagraphByLineNumber(int number)
        {
            foreach (Paragraph p in _paragraphs)
            {
                if (p.Number == number)
                    return p;
            }
            return null;
        }

        internal int RemoveEmptyLines()
        {
            int count = 0;
            if (_paragraphs.Count > 0)
            {
                int firstNumber = _paragraphs[0].Number;
                for (int i = _paragraphs.Count - 1; i >= 0; i--)
                {
                    Paragraph p = _paragraphs[i];
                    string s = p.Text.Trim();

                    if (s.Length == 0)
                    {
                        _paragraphs.RemoveAt(i);
                        count++;
                    }
                }
                Renumber(firstNumber);
            }
            return count;
        }

        /// <summary>
        /// Sort subtitle paragraphs
        /// </summary>
        /// <param name="sortCriteria">Paragraph sort criteria</param>
        public void Sort(SubtitleSortCriteria sortCriteria)
        {
            switch (sortCriteria)
            {
                case SubtitleSortCriteria.Number:
                    _paragraphs.Sort(delegate(Paragraph p1, Paragraph p2)
                    {
                        return p1.Number.CompareTo(p2.Number);
                    });
                    break;
                case SubtitleSortCriteria.StartTime:
                    _paragraphs.Sort(delegate(Paragraph p1, Paragraph p2)
                    {
                        return p1.StartTime.TotalMilliseconds.CompareTo(p2.StartTime.TotalMilliseconds);
                    });
                    break;
                case SubtitleSortCriteria.EndTime:
                    _paragraphs.Sort(delegate(Paragraph p1, Paragraph p2)
                    {
                        return p1.EndTime.TotalMilliseconds.CompareTo(p2.EndTime.TotalMilliseconds);
                    });
                    break;
                case SubtitleSortCriteria.Duration:
                    _paragraphs.Sort(delegate(Paragraph p1, Paragraph p2)
                    {
                        return p1.Duration.TotalMilliseconds.CompareTo(p2.Duration.TotalMilliseconds);
                    });
                    break;
                case SubtitleSortCriteria.Text:
                    _paragraphs.Sort(delegate(Paragraph p1, Paragraph p2)
                    {
                        return p1.Text.CompareTo(p2.Text);
                    });
                    break;
                case SubtitleSortCriteria.TextMaxLineLength:
                    _paragraphs.Sort(delegate(Paragraph p1, Paragraph p2)
                    {
                        return Utilities.GetMaxLineLength(p1.Text).CompareTo(Utilities.GetMaxLineLength(p2.Text));
                    });
                    break;
                case SubtitleSortCriteria.TextTotalLength:
                    _paragraphs.Sort(delegate(Paragraph p1, Paragraph p2)
                    {
                        return p1.Text.Length.CompareTo(p2.Text.Length);
                    });
                    break;
                case SubtitleSortCriteria.TextNumberOfLines:
                    _paragraphs.Sort(delegate(Paragraph p1, Paragraph p2)
                    {
                        return p1.NumberOfLines.CompareTo(p2.NumberOfLines);
                    });
                    break;
                case SubtitleSortCriteria.TextCharactersPerSeconds:
                    _paragraphs.Sort(delegate(Paragraph p1, Paragraph p2)
                    {
                        return Utilities.GetCharactersPerSecond(p1).CompareTo(Utilities.GetCharactersPerSecond(p2));
                    });
                    break;
                default:
                    break;
            }
        }

        internal void InsertParagraphInCorrectTimeOrder(Paragraph newParagraph)
        {
            for (int i=0; i<Paragraphs.Count; i++)
            {
                Paragraph p = Paragraphs[i];
                if (newParagraph.StartTime.TotalMilliseconds < p.StartTime.TotalMilliseconds)
                {
                    Paragraphs.Insert(i, newParagraph);
                    return;
                }
            }
            Paragraphs.Add(newParagraph);
        }
    }
}
