using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interactivity;

namespace SubtitleTranslator.Application.Utils
{
    public class RoutedEventTrigger : EventTriggerBase<DependencyObject>
    {
        public RoutedEvent RoutedEvent { get; set; }

        protected override string GetEventName()
        {
            return RoutedEvent.Name;
        }

        protected override void OnAttached()
        {
            var behavior = base.AssociatedObject as Behavior;
            var associatedElem = base.AssociatedObject as FrameworkElement;

            if(behavior != null)
            {
                associatedElem = ((IAttachedObject) behavior).AssociatedObject as FrameworkElement;
            }
            if(associatedElem == null)
            {
                throw new ArgumentException("Routed Event trigger can only be associated to framework elements");
            }

            if(RoutedEvent != null)
            {
                associatedElem.AddHandler(RoutedEvent, new RoutedEventHandler((sender, args) => base.OnEvent(args)));
            }
        }
    }
}
