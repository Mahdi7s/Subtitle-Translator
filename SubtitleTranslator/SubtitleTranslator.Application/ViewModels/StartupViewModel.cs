using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using SubtitleTranslator.Application.Contracts;
using SubtitleTranslator.Application.Models;
using SubtitleTranslator.Application.Utils;
using SubtitleTranslator.Contracts;
using SubtitleTranslator.Players;
using SubtitleTranslator.SubtitleReaders;
using TS7S.Base;

namespace SubtitleTranslator.Application.ViewModels
{
    [Export]
    public class StartupViewModel : Screen, IWindow
    {
        private readonly ITheme _theme;
        private ObservableCollection<string> _foundedSubtitlePathes;
        private ObservableCollection<PlayerModel> _players;
        private ShellViewModel _shellViewModel;
        private PlayerModel _selectedPlayer;
        private string _selectedSubtitlePath;

        [ImportingConstructor]
        public StartupViewModel(ITheme theme)
        {
            _theme = theme;
            Func<string, string, string, PlayerModel> getPlayerFunc = (pName, pIcon, pConfigName) =>
                                                                          {
                                                                              var retval = new PlayerModel { PlayerName = pName, PlayerIcon = new BitmapImage(new Uri(pIcon, UriKind.Relative)), PlayerExePath = ConfigurationSettings.AppSettings[pConfigName].Split(new[] { ',' }).Select(x => x.Trim()).ToArray() };
                                                                              retval.PropertyChanged += OnSelectedPlayerChanged;

                                                                              return retval;
                                                                          };

            _players = new ObservableCollection<PlayerModel>();
            _players.Add(getPlayerFunc("KM Player", _theme.ImagePath("KMP-256.png"), "KMPPath"));
            _players.Add(getPlayerFunc("Window Media Player", _theme.ImagePath("WMP-128.png"), "WMPPath"));

            NotifyOfPropertyChange(() => Players);
        }

        private void OnSelectedPlayerChanged(object sender, PropertyChangedEventArgs e)
        {
            //var selPlayer = (PlayerModel) sender;
            //if (selPlayer.IsSelected)
            //{
            //    foreach (var p in Players.Where(x => !x.PlayerName.Equals(selPlayer.PlayerName) && x.IsSelected))
            //    {
            //        p.IsSelected = false;
            //    }
            //}
        }

        [Import]
        public IShell Shell
        {
            get { return _shellViewModel; }
            set { _shellViewModel = (ShellViewModel)value; }
        }

        public void InitPlayer()
        {
            _shellViewModel.SubtitleReader = new AllSubtitleReader();
            _shellViewModel.PlayerController = GetPlayerController();

            var applicationComArgs = Environment.GetCommandLineArgs();
            if (!applicationComArgs.IsNullOrEmpty() && applicationComArgs.Count() > 1)
            {
                var filePath = applicationComArgs.Skip(1).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    var subtitlePath = filePath; //*******
                    SubtitlePathFinder.

                    _shellViewModel.Init(subtitlePath);
                }
            }
            else
            {
                _shellViewModel.Init();
            }
        }

        public RecentFilesModel RecentFiles { get; set; }

        public ObservableCollection<string> FoundedSubtitlePathes
        {
            get { return _foundedSubtitlePathes; }
            set
            {
                _foundedSubtitlePathes = value;
                NotifyOfPropertyChange(() => FoundedSubtitlePathes);
            }
        }

        public string SelectedSubtitlePath
        {
            get { return _selectedSubtitlePath; }
            set { _selectedSubtitlePath = value; NotifyOfPropertyChange(() => SelectedSubtitlePath); }
        }

        public ObservableCollection<PlayerModel> Players
        {
            get { return _players; }
            set
            {
                _players = value;
                NotifyOfPropertyChange(() => Players);
            }
        }

        public PlayerModel SelectedPlayer
        {
            get { return _selectedPlayer; }
            set
            {
                _selectedPlayer = value;
                NotifyOfPropertyChange(() => SelectedPlayer);
            }
        }

        private IPlayerController GetPlayerController()
        {
            if (SelectedPlayer == null) return null;
            switch (SelectedPlayer.PlayerName)
            {
                case "KM Player":
                    return new KMController();
                    break;
                case "Window Media Player":
                    return new WMPController();
                    break;
            }
            return null;
        }
    }
}
