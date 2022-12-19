using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SubtitleTranslator.Application.Controllers
{
    public class ImageButton : Button
    {
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.RegisterAttached("ImageSource", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(default(ImageSource)));

        public static void SetImageSource(ImageButton element, ImageSource value)
        {
            element.SetValue(ImageSourceProperty, value);
        }

        public static ImageSource GetImageSource(ImageButton element)
        {
            return (ImageSource)element.GetValue(ImageSourceProperty);
        }
    }
}
