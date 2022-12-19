using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SubtitleTranslator.Contracts;

namespace SubtitleTranslator.Application.ViewModels
{
    [Export(typeof(StoryboardsManager))]
    public class StoryboardsManager
    {
        public Storyboard WindowMouseLeaveStoryboard { get; set; }
        public Storyboard WindowMouseEnterStoryboard { get; set; }

        public void SetWindowLeaveBackground(Brush background)
        {
            var parent = WindowMouseLeaveStoryboard.Children.First() as ColorAnimationUsingKeyFrames;
            parent.KeyFrames[0].Value = ((SolidColorBrush)background).Color;
        }

        public void SetWindowEnterBackground(Brush background)
        {
            var parent = WindowMouseEnterStoryboard.Children.First() as ColorAnimationUsingKeyFrames;
            parent.KeyFrames[0].Value = ((SolidColorBrush)background).Color;
        }

        public void Initialize(ShellView shellView, Func<IPlayerController> getPlayerController)
        {
            WindowMouseEnterStoryboard = shellView.Resources["OnWindowMouseEnter"] as Storyboard;
            WindowMouseLeaveStoryboard = shellView.Resources["OnWindowMouseEnter_Copy1"] as Storyboard;

            WindowMouseLeaveStoryboard.Completed += (sender, args) =>
                                                        {
                                                            if (!getPlayerController().IsPlaying())
                                                                WindowMouseEnterStoryboard.Begin();
                                                        };

            WindowMouseEnterStoryboard.Begin();
        }
    }
}
