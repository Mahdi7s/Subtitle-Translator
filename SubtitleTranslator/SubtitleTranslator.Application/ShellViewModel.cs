using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using SubtitleTranslator.Application.Messages;
using TS7S.Base;
using SubtitleTranslator.Application.Commands;
using SubtitleTranslator.Application.ViewModels;
using SubtitleTranslator.Contracts;
using SubtitleTranslator.Players;
using SubtitleTranslator.SubtitleReaders;
using Timer = System.Timers.Timer;

namespace SubtitleTranslator.Application
{
    using System.ComponentModel.Composition;

    [Export(typeof(IShell))]
    public class ShellViewModel : PropertyChangedBase, IShell, IHandle<InvokeMethodMessage<ShellViewModel>>
    {
        private IPlayerController _playerController;
        private ISubtitleReader _subtitleReader;
        private Timer _timer = new Timer(1000);
        private bool _isDictionaryExpanded;
        private bool _isSettingsExpanded;
        private DictionaryViewModel _dictionary;
        private SettingsViewModel _settings;
        private string _lastSubtitle = string.Empty;
        private string[] _lastSubtitleArr = null;
        private bool _isPlaying = true;
        private bool _isInTranslationDelay = false;
        public bool _draggingSlider = false;
        public bool? _wasPlayingBeforeDelay = null;
        private double _spentDelaySeconds = 0;
        private TimeSpan _currentPosition = TimeSpan.Zero;

        public ShellViewModel()
        {
            _playerController = new KMController();
            _subtitleReader = new AllSubtitleReader();

            var applicationComArgs = Environment.GetCommandLineArgs();
            if (!applicationComArgs.IsNullOrEmpty() && applicationComArgs.Count() > 1)
            {
                var filePath = applicationComArgs.Skip(1).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    var subtitlePath = filePath; //*******
                    Init(subtitlePath);
                }
            }
            Init();
        } 

        public IPlayerController PlayerController
        {
            get { return _playerController; }
            set { _playerController = value; }
        }

        public ISubtitleReader SubtitleReader
        {
            get { return _subtitleReader; }
            set { _subtitleReader = value; }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var playing = _playerController.IsPlaying();

            if (_isPlaying != playing)
            {
                _isPlaying = playing;
                NotifyOfPropertyChange(() => PlayPauseImage);
            }

            if (_isPlaying && !_draggingSlider)
            {
                NotifyOfPropertyChange(() => CurrentPosition);
                NotifyOfPropertyChange(() => SliderValue);
                NotifyOfPropertyChange(() => Subtitle);
            }

            if (_isInTranslationDelay)
            {
                _spentDelaySeconds += (_timer.Interval / 1000);
                if (_spentDelaySeconds >= Settings.SelectedTranslationDelay)
                {
                    if (_wasPlayingBeforeDelay == true)
                    {
                        _playerController.PlayOrPause();
                    }
                    _wasPlayingBeforeDelay = null;
                    _isInTranslationDelay = false;
                }
            }
        }

        public bool ShowStartupChooseView
        {
            get { return _showStartupChooseView; }
            set
            {
                _showStartupChooseView = value;
                NotifyOfPropertyChange(() => ShowStartupChooseView);
            }
        }

        [Import]
        public StartupViewModel StartupChooseScreen
        {
            get { return _startupChooseScreen; }
            set { _startupChooseScreen = value;
                NotifyOfPropertyChange(()=>StartupChooseScreen); }
        }

        public ShellView View { get; private set; }

        public WindowState WindowState
        {
            get { return _windowState; }
            set { _windowState = value; NotifyOfPropertyChange(() => WindowState); }
        }

        [Import]
        public StoryboardsManager StoryboardsManager { get; set; }

        [Import]
        public DictionaryViewModel Dictionary
        {
            get { return _dictionary; }
            set { _dictionary = value; NotifyOfPropertyChange(() => Dictionary); }
        }

        [Import]
        public SettingsViewModel Settings
        {
            get { return _settings; }
            set { _settings = value; NotifyOfPropertyChange(() => Settings); }
        }

        public TimeSpan CurrentPosition
        {
            get
            {
                if (!_draggingSlider)
                {
                    _currentPosition = _playerController.GetCurrentPosition();
                }
                return _currentPosition;
            }
            set
            {
                _currentPosition = value;
                if (!_draggingSlider)
                {
                    _playerController.Seek(_currentPosition); 
                }
                else
                {
                    NotifyOfPropertyChange(() => Subtitle);
                }
            }
        }

        public TimeSpan Duration
        {
            get { return _playerController.GetDuration(); }
        }

        public double SliderMax
        {
            get { return Duration.TotalSeconds; }
        }

        public double SliderValue
        {
            get { return CurrentPosition.TotalSeconds; }
            set { CurrentPosition = TimeSpan.FromSeconds(value); }
        }

        public bool IsDictionaryExpanded
        {
            get { return _isDictionaryExpanded; }
            set
            {
                _isDictionaryExpanded = value;
                NotifyOfPropertyChange(() => IsDictionaryExpanded);
            }
        }

        public bool IsSettingsExpanded
        {
            get { return _isSettingsExpanded; }
            set
            {
                _isSettingsExpanded = value;
                NotifyOfPropertyChange(() => IsSettingsExpanded);
            }
        }

        public ISubtitleFrame SubtitleDetail { get; private set; }

        public string[] Subtitle
        {
            get
            {
                SubtitleDetail = _subtitleReader.SubtitleDetailsOf(CurrentPosition);
                var newSubtitle = SubtitleDetail != null ? SubtitleDetail.Text : string.Empty;
                   
                if (!newSubtitle.Equals(_lastSubtitle))
                {
                    _lastSubtitle = newSubtitle;
                    _lastSubtitleArr = newSubtitle.Replace("\n", " ").Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
                }

                return _lastSubtitleArr;
            }
        }

        private readonly ImageSource _stopImage = new BitmapImage(new Uri("/Styles/LightDark/Images/pause.png", UriKind.Relative));
        private readonly ImageSource _playImage = new BitmapImage(new Uri("/Styles/LightDark/Images/play.png", UriKind.Relative));
        private WindowState _windowState;
        private bool _showStartupChooseView = true;
        private StartupViewModel _startupChooseScreen;

        public ImageSource PlayPauseImage
        {
            get { return _isPlaying ? _stopImage : _playImage; }
        }

        [Import]
        public PlayOrPauseCommand PlayOrPauseCommand { get; set; }

        public void OnSliderDragStarted()
        {
            _draggingSlider = true;
        }

        public void OnSliderDragCompleted()
        {
            _draggingSlider = false;
            _playerController.Seek(_currentPosition);
        }

        public void StartDelay()
        {
            if (!Settings.PauseWhileTranslation) return;

            if (!_isInTranslationDelay && ((_wasPlayingBeforeDelay = _isPlaying) == true))
            {
                _playerController.PlayOrPause();
            }
            _isInTranslationDelay = true;
            _spentDelaySeconds = 0;
        }

        public void Next()
        {
            _playerController.NextTrack();
        }

        public void Prev()
        {
            _playerController.PrevTrack();
        }

        public void PlayOrPause()
        {
            _playerController.PlayOrPause();
            Settings.TopMost = true;
        }

        public void Close()
        {
            Environment.Exit(Environment.ExitCode);
        }

        public void Minimize()
        {
            this.WindowState = WindowState.Minimized;
        }

        public void OnViewLoaded(ShellView view)
        {
            this.View = view;
            StoryboardsManager.Initialize(view, () => _playerController);
        }

        public void OnWindowLeave()
        {
            IsDictionaryExpanded = IsSettingsExpanded = false;
        }

        public void OnFileDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var file =
                    files.FirstOrDefault();// (x => Path.HasExtension(x) && Path.GetExtension(x).Equals(".srt", StringComparison.OrdinalIgnoreCase));

                Init(file);
            }
        }

        public void OnWordClick(object sender)
        {
            IsSettingsExpanded = false;
            IsDictionaryExpanded = true;

            var txtBlock = sender as TextBlock;
            Dictionary.Search(txtBlock.Text);
        }

        public void OnMouseLeftDown(MouseButtonEventArgs e)
        {
            IsDictionaryExpanded = IsSettingsExpanded = false;

            this.View.DragMove();
        }

        internal void Init(string subtitlePath = null)
        {
            _playerController = new KMController();
            
            if (!string.IsNullOrEmpty(subtitlePath))
            {
                Debug.Fail("Path: " + subtitlePath);
                _subtitleReader = new AllSubtitleReader { SubtitlePath = subtitlePath };
                _subtitleReader.ReadSubtitle();
            }

            NotifyOfPropertyChange(() => Duration);
            NotifyOfPropertyChange(() => SliderMax);

            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }

        public void Handle(InvokeMethodMessage<ShellViewModel> message)
        {
            message.Invoke(this);
        }
    }
}
