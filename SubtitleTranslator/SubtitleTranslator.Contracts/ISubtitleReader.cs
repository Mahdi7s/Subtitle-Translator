using System;

namespace SubtitleTranslator.Contracts
{
    public interface ISubtitleReader
    {
        string SubtitlePath { get; }
        void ReadSubtitle();
        string SubtitleOf(TimeSpan time);
        ISubtitleFrame SubtitleDetailsOf(TimeSpan time);
    }
}