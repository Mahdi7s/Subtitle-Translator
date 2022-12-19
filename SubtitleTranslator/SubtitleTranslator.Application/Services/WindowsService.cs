using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using SubtitleTranslator.Application.Contracts;
using SubtitleTranslator.Application.Messages;

namespace SubtitleTranslator.Application.Services
{
    public enum AppWindows{ShellWindow, StartupWindow, SettingsWindow, LinguaWindow}

    [Export]
    public class WindowsService : IService, IHandle<ShowWindowMessage>
    {
        private readonly IWindowManager _windowManager;
        private readonly IEnumerable<IWindow> _windows;

        [ImportingConstructor]
        public WindowsService(IWindowManager windowManager, [ImportMany]IEnumerable<IWindow> windows)
        {
            _windowManager = windowManager;
            _windows = windows;
        }

        public bool? Show(AppWindows window, WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen, bool showAsDialog = false)
        {
            bool? retval = null;

            var windowToShow = _windows.FirstOrDefault(x => x.WindowName.Equals(window.ToString()));
            if(windowToShow == null) throw new Exception("Can not find window " + window.ToString());
            dynamic settings = new ExpanadoObject();
            settings.WindowStartupLocation = startupLocation;

            if(showAsDialog)
            {
                retval = _windowManager.ShowDialog(windowToShow, null, settings);
            }
            else
            {
                _windowManager.ShowWindow(windowToShow, null, settings);
            }

            return retval;
        }

        public void Handle(ShowWindowMessage message)
        {
            Show(message.Window, showAsDialog: message.AsDialog);
        }
    }
}
