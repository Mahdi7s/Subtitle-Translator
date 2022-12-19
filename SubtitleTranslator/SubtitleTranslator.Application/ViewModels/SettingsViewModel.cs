using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using SubtitleTranslator.Application.Contracts;
using SubtitleTranslator.Application.Services;

namespace SubtitleTranslator.Application.ViewModels
{
    [Export(typeof(SettingsViewModel))]
    public class SettingsViewModel : Screen, IWindow
    {
        public SettingsViewModel()
        {
            FontSizes = Enumerable.Range(1, 24).Select(x => x * 2).ToArray();
            TranslationDelays = Enumerable.Range(1, 4).Select(x => x * 5).ToArray();

            AppBrushes =
                typeof(Brushes).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(
                    x => x.Name).ToArray();

            _windowLeaveBackground = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            _windowBackground = Brushes.White;

            _subtitleFont = new FontFamily("Arial");
            _subtitleFontSize = 12;
            _subtitleForeground = Brushes.Black;

            _selectedTranslationDelay = 5;
            _pauseWhileTranslation = true;

            TopMost = true;
        }

        private Brush _windowBackground;
        private Brush _windowLeaveBackground;
        private Brush _subtitleForeground;
        private FontFamily _subtitleFont;
        private int _subtitleFontSize;
        private bool _topMost;
        private IShell _shellViewModel;
        private bool _pauseWhileTranslation;
        private int _selectedTranslationDelay;

        public int[] FontSizes { get; set; }
        public string[] AppBrushes { get; set; }

        [Import]
        public StoryboardsManager StoryboardsManager { get; set; }

        [Import]
        public IShell ShellViewModel
        {
            get { return _shellViewModel; }
            set { _shellViewModel = value; }
        }

        public Brush WindowBackground
        {
            get { return _windowBackground; }
            set
            {
                _windowBackground = value;
                ((ShellViewModel)ShellViewModel).View.Background = value;
                StoryboardsManager.SetWindowEnterBackground(value);

                NotifyOfPropertyChange(() => WindowBackground);
            }
        }

        public Brush WindowLeaveBackground
        {
            get { return _windowLeaveBackground; }
            set
            {
                _windowLeaveBackground = value;
                StoryboardsManager.SetWindowLeaveBackground(value);

                NotifyOfPropertyChange(() => WindowLeaveBackground);
            }
        }

        public Brush SubtitleForeground
        {
            get { return _subtitleForeground; }
            set
            {
                _subtitleForeground = value;
                NotifyOfPropertyChange(() => SubtitleForeground);
            }
        }

        public FontFamily SubtitleFont
        {
            get { return _subtitleFont; }
            set
            {
                _subtitleFont = value;
                NotifyOfPropertyChange(() => SubtitleFont);
            }
        }

        public int SubtitleFontSize
        {
            get { return _subtitleFontSize; }
            set
            {
                _subtitleFontSize = value;
                NotifyOfPropertyChange(() => SubtitleFontSize);
            }
        }

        public bool TopMost
        {
            get { return _topMost; }
            set
            {
                _topMost = value;
                NotifyOfPropertyChange(() => TopMost);
            }
        }

        public bool PauseWhileTranslation
        {
            get { return _pauseWhileTranslation; }
            set { _pauseWhileTranslation = value; NotifyOfPropertyChange(() => PauseWhileTranslation); }
        }

        public int[] TranslationDelays { get; private set; }

        public int SelectedTranslationDelay
        {
            get { return _selectedTranslationDelay; }
            private set { _selectedTranslationDelay = value; NotifyOfPropertyChange(() => SelectedTranslationDelay); }
        }
    }
}
