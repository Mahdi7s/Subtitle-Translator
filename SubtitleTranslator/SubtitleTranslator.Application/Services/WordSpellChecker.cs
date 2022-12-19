using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.IO;
using NHunspell;
using TS7S.Base;
using TS7S.Base.IO;

namespace SubtitleTranslator.Application.Services
{
    [Export]
    public class WordSpellChecker
    {
        private readonly Hunspell _hunspell;

        public WordSpellChecker()
        {
            var appDir = Path.GetDirectoryName(typeof(WordSpellChecker).Assembly.Location);
            var affPath = Path.Combine(appDir, @"Dictionary\en_US.aff");
            var dicPath = Path.Combine(appDir, @"Dictionary\en_US.dic");

            _hunspell = new Hunspell(affPath, dicPath);
        }

        public IEnumerable<string> GetStemOrSuggest(string word)
        {
            var stems = _hunspell.Stem(word);
            if (stems.IsNullOrEmpty() || (stems.Count == 1 && stems[0].Equals(word, StringComparison.InvariantCultureIgnoreCase)))
            {
                return _hunspell.Suggest(word);
            }
            return stems.Where(x=>!x.Equals(word, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }
    }
}
