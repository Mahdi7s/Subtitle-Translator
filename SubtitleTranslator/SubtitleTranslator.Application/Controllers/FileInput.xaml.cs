using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Caliburn.Micro;
using SubtitleTranslator.Application.Services;

namespace SubtitleTranslator.Application.Controllers
{
    public enum FilterType { Movies, Subtitles, All}

    /// <summary>
    /// Interaction logic for FileInput.xaml
    /// </summary>
    public partial class FileInput : UserControl, INotifyPropertyChanged
    {
        private string _filePath;
        private FileBrowserService _fileBrowserService;
        private FilterType _filterType = FilterType.All;

        public FileInput()
        {
            InitializeComponent();

            DataContext = this;

            _fileBrowserService = IoC.Get<FileBrowserService>();
        }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged("FilePath");
            }
        }

        public FilterType FilterType
        {
            get { return _filterType; }
            set { _filterType = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (_filterType == FilterType.Movies)
            {
                FilePath = _fileBrowserService.BrowseMovies();
            }
            else if (_filterType == FilterType.Subtitles)
            {
                FilePath = _fileBrowserService.BrowseSubtitles();
            }
            else
            {
                FilePath = _fileBrowserService.Browse("*.*", "انتخاب فایل");
            }
        }
    }
}
