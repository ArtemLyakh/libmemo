using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Libmemo {
    public static class Settings {
        private static ISettings AppSettings {
            get {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants
        private const string _lastModified = "modified";
        private static readonly long _lastModifiedDefault = 0;

        private const string _dataUrl = "http://libmemo.com/api/data.php";

        private const string _databaseName = "database.db";

        private const string _addPersonUrl = "http://libmemo.com/api/add.php";
        #endregion


        public static long? LastModified {
            get {
                long res = AppSettings.GetValueOrDefault<long>(_lastModified, _lastModifiedDefault);
                return res == 0 ? null : (long?)res;
            }
            set {
                if (value != null) {
                    AppSettings.AddOrUpdateValue<long>(_lastModified, (long)value);
                } else {
                    AppSettings.AddOrUpdateValue<long>(_lastModified, _lastModifiedDefault);
                }
            }
        }


        public static string DataUrl { get; } = _dataUrl;
        public static string AddPersonUrl { get; } = _addPersonUrl;

        public static string DatabaseName { get; } = _databaseName;

    }
}