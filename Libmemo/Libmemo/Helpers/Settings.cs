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
        private const string SERVER_URL = "http://libmemo.com";

        public const string LOGIN_URL = SERVER_URL + "/api/auth/login.php";

        public const string REGISTER_URL = SERVER_URL + "/api/auth/register.php";
        public const string REGISTER_URL_ADMIN = SERVER_URL + "/api/auth/register_admin.php";

        public const string PERSONAL_DATA_URL = SERVER_URL + "/api/personal/personal_data.php";
        public const string PERSONAL_DATA_URL_ADMIN = SERVER_URL + "/api/personal/personal_data_admin.php";

        public const string ADD_PERSON_URL = SERVER_URL + "/api/database/add.php";
        public const string ADD_PERSON_URL_ADMIN = SERVER_URL + "/api/database/add_admin.php";



        public const string DATABASE_URL = SERVER_URL + "/api/database/database.php";

        


        

        private const string EDIT_PERSON_URL = SERVER_URL + "/api/edit.php";
        private const string EDIT_PERSON_URL_ADMIN = SERVER_URL + "/api/admin/edit.php";

        
        #endregion


        private static readonly long _lastModifiedDefault = 0;
        public static long? LastModified {
            get {
                long res = AppSettings.GetValueOrDefault("modified", _lastModifiedDefault);
                return res == 0 ? null : (long?)res;
            }
            set {
                if (value != null) {
                    AppSettings.AddOrUpdateValue("modified", (long)value);
                } else {
                    AppSettings.AddOrUpdateValue("modified", _lastModifiedDefault);
                }
            }
        }


        


        public static string Email {
            get { return AppSettings.GetValueOrDefault<string>("login", null); }
            set { AppSettings.AddOrUpdateValue("login", value); }
        }
        public static string Password {
            get { return AppSettings.GetValueOrDefault<string>("password", null); }
            set { AppSettings.AddOrUpdateValue("password", value); }
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
                    cookieContainer.Add(new Uri(SERVER_URL), cookieCollection);

                    return cookieContainer;
                }
            }
            set {
                if (value == null) AppSettings.AddOrUpdateValue<string>("authCookies", null);
                else {
                    List<Cookie> cookieList = new List<Cookie>();
                    foreach (var item in value.GetCookies(new Uri(SERVER_URL))) {
                        Cookie cookie = (Cookie)item;
                        cookieList.Add(cookie);
                    }

                    var str = JsonConvert.SerializeObject(cookieList);

                    AppSettings.AddOrUpdateValue<string>("authCookies", str);
                }

            }
        }

        public static int CurrentUser {
            get { return AppSettings.GetValueOrDefault("currentuser", default(int)); }
            set { AppSettings.AddOrUpdateValue("currentuser", value); }
        }










        public static string EditPersonUrl { get; } = EDIT_PERSON_URL;
        public static string EditPersonUrlAdmin { get; } = EDIT_PERSON_URL_ADMIN;

    }

}