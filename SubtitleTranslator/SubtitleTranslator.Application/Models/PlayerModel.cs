using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Caliburn.Micro;

namespace SubtitleTranslator.Application.Models
{
    public class PlayerModel : PropertyChangedBase
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }

        public string PlayerName { get; set; }
        public string[] PlayerExePath { get; set; }
        public ImageSource PlayerIcon { get; set; }
    }
}
