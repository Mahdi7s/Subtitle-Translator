using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SubtitleTranslator.Application
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		public Window1()
		{
			this.InitializeComponent();

		    pnlText.ItemsSource =
		        "Insert code required on object creation below this point.".Split(' ').Where(
		            x => !string.IsNullOrWhiteSpace(x)).ToArray();
		}

        private void LayoutRoot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void LayoutRoot_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void LayoutRoot_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Hyperlink_MouseDown_1(object sender, MouseButtonEventArgs e)
        {

        }

        private void Hyperlink_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {

        }

        private void TextBlock_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {

        }

        private void Slider_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {

        }

        private void Thumb_MouseDown_1(object sender, MouseButtonEventArgs e)
        {

        }

        private void Slider_MouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {

        }

        private void Slider_DragStarted_1(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void Slider_DragCompleted_1(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }
	}
}