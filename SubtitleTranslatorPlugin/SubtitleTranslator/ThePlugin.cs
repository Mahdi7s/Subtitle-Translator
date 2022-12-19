using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SubtitleTranslator
{
    public class ThePlugin : GeneralPlugin
    {
        private PluginSettings _settings = null;
        public PluginSettings Settings
        {
            get { return (_settings = _settings ?? PluginSettings.Load()); }
        }

        public override string Name
        {
            get { return "Subtitle Translator"; }
        }

        public override void Initialize()
        {
            //Winamp.SongChanged += (sender, args) =>
            //{
            //    OnShowingMovie("SongChanged");
            //};
            OnShowingMovie();
        }

        private void OnShowingMovie()
        {
            Process.Start(Settings.ProgramPath, Winamp.CurrentSong.Filename);
        }

        public override void Config()
        {
            
        }

        public override void Quit()
        {

        }
    }
}
