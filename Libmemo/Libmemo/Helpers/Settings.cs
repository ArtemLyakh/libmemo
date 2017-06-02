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

        private const string _dataUrl = "http://libmemo.com/api/database.php";

        private const string _databaseName = "database.db";

        private const string _addPersonUrl = "http://libmemo.com/api/add.php";
        private const string _addPersonAdminUrl = "http://libmemo.com/api/admin/add.php";

        private const string _loginUri = "http://libmemo.com/api/login.php";
        private const string _registerUri = "http://libmemo.com/api/register.php";
        private const string _personalDataSend = "http://libmemo.com/api/personal-data-send.php";
        private const string _personalDataGet = "http://libmemo.com/api/personal-data-get.php";
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



        public static bool Logged {
            get { return AppSettings.GetValueOrDefault<bool>("logged", false); }
            set { AppSettings.AddOrUpdateValue<bool>("logged", value); }
        }
        public static string Email {
            get { return AppSettings.GetValueOrDefault<string>("login", null); }
            set { AppSettings.AddOrUpdateValue("login", value); }
        }
        public static string Password {
            get { return AppSettings.GetValueOrDefault<string>("password", null); }
            set { AppSettings.AddOrUpdateValue("password", value); }
        }
        public static UserType UserType {
            get { return (UserType)AppSettings.GetValueOrDefault<int>("usertype", default(int)); }
            set { AppSettings.AddOrUpdateValue("usertype", (int)value); }
        }













        public static string DataUrl { get; } = _dataUrl;
        public static string AddPersonUrl { get; } = _addPersonUrl;
        public static string AddPersoAdminUrl { get; } = _addPersonAdminUrl;
        public static string PersonalDataSend { get; } = _personalDataSend;
        public static string PersonalDataGet { get; } = _personalDataGet;

        public static string DatabaseName { get; } = _databaseName;

        public static string LoginUri { get; } = _loginUri;

        public static string RegisterUri { get; } = _registerUri;
    }

}