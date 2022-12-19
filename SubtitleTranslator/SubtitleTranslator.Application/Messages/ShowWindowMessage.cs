using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubtitleTranslator.Application.Services;

namespace SubtitleTranslator.Application.Messages
{
    public class ShowWindowMessage
    {
        public AppWindows Window { get; set; }
        public bool AsDialog { get; set; }
    }
}
