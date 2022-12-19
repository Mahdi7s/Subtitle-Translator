using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubtitleTranslator.Contracts
{
    public interface ISubtitleFrame
    {
        TimeSpan Start { get; set; }
        TimeSpan End { get; set; }
        string Text { get; set; }
    }
}
