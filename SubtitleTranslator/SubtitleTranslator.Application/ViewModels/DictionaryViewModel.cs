using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using SubtitleTranslator.Application.Messages;
using SubtitleTranslator.Application.Services;
using SubtitleTranslator.DataModel;
using TS7S.Base;
using TS7S.Base.Threading;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using SubtitleTranslator.Application.Commands;
using SubtitleTranslator.Application.Models;
using System.Data.Entity.Infrastructure;

namespace SubtitleTranslator.Application.ViewModels
{
    [Export]
    public class DictionaryViewModel : Screen, IHandle<PlayingMovieMessage>
    {
        private readonly IEventAggregator _eventAggregator;
        private string _word;
        private ObservableCollection<string> _results = new ObservableCollection<string>();
        private PlayingMovieMessage _currentMovieInfo;
        private SubtitleMovie _currentSubtitleMovie;

        private Visibility _suggestsVisibility = Visibility.Collapsed;
        private ObservableCollection<string> _suggests = new ObservableCollection<string>();

        [ImportingConstructor]
        public DictionaryViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        [Import]
        public SearchCommand SearchCommand { get; set; }

        [Import]
        public UnitOfWork UnitOfWork { get; set; }

        [Import]
        public WordSpellChecker SpellChecker { get; set; }

        public string Word
        {
            get { return _word; }
            set
            {
                _word = value;
                NotifyOfPropertyChange(() => Word);
            }
        }

        public Visibility SuggestsVisibility
        {
            get { return Suggests.IsNullOrEmpty() ? Visibility.Collapsed : Visibility.Visible; }
        }

        public ObservableCollection<string> Suggests
        {
            get { return _suggests; }
            set
            {
                _suggests = value;
                NotifyOfPropertyChange(() => Suggests);
                NotifyOfPropertyChange(() => SuggestsVisibility);
            }
        }

        public ObservableCollection<string> Results
        {
            get { return _results; }
            set
            {
                _results = value;
                NotifyOfPropertyChange(() => Results);
            }
        }

        public void OnSuggestClick(object sender)
        {
            var txtBlock = sender as TextBlock;
            Search(txtBlock.Text);
        }

        public void Search(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return;

            //-----------------------------------------------

            Word = word;
            _eventAggregator.Publish(new InvokeMethodMessage<ShellViewModel> {MethodName = "StartDelay"});

            ThreadHelper.RunAsync(() =>
            {
                if(_currentSubtitleMovie==null) _currentSubtitleMovie = UnitOfWork.SubtitleMovieRep.UnknownMovie;

                var dbFinds = UnitOfWork.DictionaryRep.GetAll().Where(x => x.OrginalWord.Equals(Word, StringComparison.InvariantCultureIgnoreCase)).Distinct().ToList();
                if (dbFinds.IsNullOrEmpty())
                {
                    Results = null; 
                    Suggests = new ObservableCollection<string>(SpellChecker.GetStemOrSuggest(Word));
                }
                else
                {
                    Suggests = null;
                    Results = new ObservableCollection<string>(dbFinds.Select(x => x.PersianWord));
                }
            });
        }

        public void AddToLightner()
        {
            if (string.IsNullOrEmpty(Word) || Results.IsNullOrEmpty()) return;

            ThreadHelper.RunAsync(() =>
            {
                var shellViewModel = ShellViewModel as ShellViewModel;
                var newSubtitle = new DataModel.Subtitle { EnWord = Word, PeWord = Results.First(), SubtitleMovieId = _currentSubtitleMovie.SubtitleMovieId };
                if (shellViewModel.SubtitleDetail != null)
                {
                    newSubtitle.StartTime = new DateTime(shellViewModel.SubtitleDetail.Start.Ticks);
                    newSubtitle.EndTime = new DateTime(shellViewModel.SubtitleDetail.End.Ticks);
                    newSubtitle.Paragraph = shellViewModel.SubtitleDetail.Text;
                }
                UnitOfWork.SubtitleRep.Insert(newSubtitle);
                UnitOfWork.SaveChanges();
            });
        }

        public void Handle(PlayingMovieMessage message)
        {
            if (message == null || message.Equals(_currentMovieInfo)) return;

            _currentMovieInfo = message;
            _currentSubtitleMovie = UnitOfWork.SubtitleMovieRep.GetAll().FirstOrDefault(x => x.MoviePath == _currentMovieInfo.MoviePath || x.SubtitlePath == _currentMovieInfo.SubtitlePath);
            if (_currentSubtitleMovie == null)
            {
                _currentSubtitleMovie = new SubtitleMovie { SubtitlePath = _currentMovieInfo.SubtitlePath, MoviePath = _currentMovieInfo.MoviePath };
                UnitOfWork.SubtitleMovieRep.Insert(_currentSubtitleMovie);
                UnitOfWork.SaveChanges();
            }
        }
    }
}
