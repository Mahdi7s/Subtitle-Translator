using System.ComponentModel.Composition;
using System.Windows.Media;
using SubtitleTranslator.Application.Contracts;

namespace SubtitleTranslator.Application.Styles.LightDark
{
    [Export(typeof(ITheme))]
    public class LightDarkTheme : ThemeBase
    {
        public override string ThemeName { get { return "LightDark"; } }
    }
}