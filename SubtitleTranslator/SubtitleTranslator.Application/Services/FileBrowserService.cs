using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using SubtitleTranslator.Application.Contracts;

namespace SubtitleTranslator.Application.Services
{
    [Export]
    public class FileBrowserService : IService
    {
        private readonly string _moivesFilter;
        private readonly string _subtitlesFilter;

        public FileBrowserService()
        {
            
        }

        public string BrowseMovies()
        {
            return Browse(_moivesFilter, "انتخاب فیلم");
        }

        public string BrowseSubtitles()
        {
            return Browse(_subtitlesFilter, "انتخاب زیرنویس");
        }

        public string Browse(string filter, string title)
        {
            var openFileDlg = new OpenFileDialog
                                  {
                                      CheckFileExists = true,
                                      CheckPathExists = true,
                                      Title = title,
                                      Filter = filter,
                                      RestoreDirectory = true,
                                      Multiselect = false
                                  };

            return openFileDlg.ShowDialog() == true ? openFileDlg.FileName : null;
        }
    }
}
