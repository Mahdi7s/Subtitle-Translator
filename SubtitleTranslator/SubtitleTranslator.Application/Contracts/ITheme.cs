using System.Windows.Media;

namespace SubtitleTranslator.Application.Contracts
{
    public interface ITheme
    {
        string ThemeName { get; }

        string ImagePath(string imageName);
        ImageSource ImageSource(string imageName);
    }
}