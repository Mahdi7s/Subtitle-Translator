using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Caliburn.Micro;
using SubtitleTranslator.Application.Contracts;
using SubtitleTranslator.Application.ViewModels;
using TS7S.Base.IO;

namespace SubtitleTranslator.Application.Services
{
    [Export]
    public class SettingsStorageService : IService
    {
        private readonly SettingsViewModel _settingsViewModel;
        private readonly string _settingsPath;
        private readonly string _recentFilesPath;

        [ImportingConstructor]
        public SettingsStorageService(SettingsViewModel settingsViewModel)
        {
            _settingsViewModel = settingsViewModel;
        }

        public SettingsStorageService()
        {
            var storageDir = Path.Combine(Path.GetDirectoryName(typeof (IShell).Assembly.Location), "Settings");

            _settingsPath = Path.Combine(storageDir, "sub.xml");
            _recentFilesPath = Path.Combine(storageDir, "recents.xml");
        }

        public void LoadSettings()
        {
            if (File.Exists(_settingsPath))
            {
                using (var strm = new FileStream(_settingsPath, FileMode.Open, FileAccess.Read))
                {
                    var provider = ObjectSerializer<SettingsViewModel>.DeserializeXml(strm);
                    SetObjectPropertis(_settingsViewModel, provider);
                }
            }
        }

        public void SaveSettings()
        {
            using (var strm = new FileStream(_settingsPath, FileMode.Create, FileAccess.Write))
            {
                ObjectSerializer<SettingsViewModel>.SerializeXml(strm, _settingsViewModel);
            }
        }

        private void SetObjectPropertis(object obj, object providerObj)
        {
            var objType = obj.GetType();
            var providerType = providerObj.GetType();

            providerType.GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty)
                .Apply(p =>
                           {
                               objType.InvokeMember(p.Name, BindingFlags.Public | BindingFlags.SetProperty, null, obj, new object[]{p.GetValue(providerObj, new object[]{})});
                           });
        }
    }
}