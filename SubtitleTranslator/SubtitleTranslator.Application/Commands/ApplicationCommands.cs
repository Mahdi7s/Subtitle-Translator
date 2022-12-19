using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows.Input;
using SubtitleTranslator.Application.ViewModels;

namespace SubtitleTranslator.Application.Commands
{
    [Export]
    public sealed class PlayOrPauseCommand : ICommand
    {
        [Import]
        public IShell ShellViewModel { get; set; }

        public void Execute(object parameter)
        {
            ((ShellViewModel)ShellViewModel).PlayOrPause();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }

    [Export]
    public sealed class SearchCommand : ICommand
    {
        [Import]
        public DictionaryViewModel DictionaryViewModel { get; set; } 

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            DictionaryViewModel.Search(DictionaryViewModel.Word);
        }
    }
}
