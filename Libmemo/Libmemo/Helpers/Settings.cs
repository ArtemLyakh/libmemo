using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Libmemo {
    public static class Settings {
        private static ISettings AppSettings {
            get {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants
        private const string SERVER_URI = "http://libmemo.com";

        private static readonly long _lastModifiedDefault = 0;

        private const string _dataUrl = "http://libmemo.com/api/data.php";

        private const string _databaseName = "database.db";

        private const string _addPersonUrl = "http://libmemo.com/api/add.php";
        #endregion


        public static long? LastModified {
            get {
                long res = AppSettings.GetValueOrDefault<long>("modified", _lastModifiedDefault);
                return res == 0 ? null : (long?)res;
            }
            set {
                if (value != null) {
                    AppSettings.AddOrUpdateValue<long>("modified", (long)value);
                } else {
                    AppSettings.AddOrUpdateValue<long>("modified", _lastModifiedDefault);
                }
            }
        }


        public static CookieContainer AuthCookies {
            get {
                var str = AppSettings.GetValueOrDefault<string>("authCookies", null);
                if (string.IsNullOrWhiteSpace(str)) return null;
                else {
                    List<Cookie> cookieList = JsonConvert.DeserializeObject<List<Cookie>>(str);
                    CookieCollection cookieCollection = new CookieCollection();
                    foreach (var cookie in cookieList) {
                        cookieCollection.Add(cookie);
                    }

                    CookieContainer cookieContainer = new CookieContainer();
                    cookieContainer.Add(new Uri(SERVER_URI), cookieCollection);

                    return cookieContainer;
                }
            }
            set {
                if (value == null) AppSettings.AddOrUpdateValue<string>("authCookies", null);
                else {
                    List<Cookie> cookieList = new List<Cookie>();
                    foreach (var item in value.GetCookies(new Uri(SERVER_URI))) {
                        Cookie cookie = (Cookie)item;
                        cookieList.Add(cookie);
                    }

                    var str = JsonConvert.SerializeObject(cookieList);

                    AppSettings.AddOrUpdateValue<string>("authCookies", str);
                }

            }
        }


        public static string DataUrl { get; } = _dataUrl;
        public static string AddPersonUrl { get; } = _addPersonUrl;

        public static string DatabaseName { get; } = _databaseName;

    }
}