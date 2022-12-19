using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SubtitleTranslator.Application.Contracts;

namespace SubtitleTranslator.Application.Styles
{
    public abstract class ThemeBase : ITheme
    {
        public abstract string ThemeName { get; }

        public virtual string ImagePath(string imageName)
        {
            return string.Format("/Styles/{0}/Images/{1}", ThemeName, imageName);
        }

        public virtual ImageSource ImageSource(string imageName)
        {
            return new BitmapImage(new Uri(ImagePath(imageName), UriKind.Relative));
        }
    }
}