using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Libmemo {
    public static class Settings {
        private static ISettings AppSettings {
            get => CrossSettings.Current;
        }

        #region Setting Constants
        public const string SERVER_URL = "http://libmemo.com";

        public const string DATABASE_URL = SERVER_URL + "/api/database/database.php";

        public const string USER_LIST_URL = SERVER_URL + "/api/admin/user_list.php";


        public const string LOGIN_URL = SERVER_URL + "/api/auth/login.php";

        public const string REGISTER_URL = SERVER_URL + "/api/auth/register.php";
        public const string REGISTER_URL_ADMIN = SERVER_URL + "/api/auth/register_admin.php";

        public const string PERSONAL_DATA_URL = SERVER_URL + "/api/personal/personal_data.php";
        public const string PERSONAL_DATA_URL_ADMIN = SERVER_URL + "/api/personal/personal_data_admin.php";

        public const string ADD_PERSON_URL = SERVER_URL + "/api/database/add.php";
        public const string ADD_PERSON_URL_ADMIN = SERVER_URL + "/api/database/add_admin.php";



        

        


        

        private const string EDIT_PERSON_URL = SERVER_URL + "/api/edit.php";
        private const string EDIT_PERSON_URL_ADMIN = SERVER_URL + "/api/admin/edit.php";


        #endregion


        public static long? LastModified {
            get {
                long res = AppSettings.GetValueOrDefault("modified", default(long));
                return res == default(long) ? null : (long?)res;
            }
            set {
                if (value != null) {
                    AppSettings.AddOrUpdateValue("modified", (long)value);
                } else {
                    AppSettings.AddOrUpdateValue("modified", default(long));
                }
            }
        }

        private static bool _authInfoCacheNeedReset = true;
        private static AuthInfo _authInfoCache = null;
        public static AuthInfo AuthInfo {
            get {
                if (_authInfoCacheNeedReset) {
                    var str = AppSettings.GetValueOrDefault<string>("auth_data", null);
                    if (string.IsNullOrWhiteSpace(str)) return null;

                    _authInfoCache = AuthInfo.Deserialize(str);
                    _authInfoCacheNeedReset = false;
                }

                return _authInfoCache;
            }
            set {
                _authInfoCacheNeedReset = true;
                AppSettings.AddOrUpdateValue("auth_data", value?.Serialize());          
            }
        }


        private static bool _authCredentialsCacheNeedReset = true;
        private static AuthCredentials _authCredentialsCache = null;
        public static AuthCredentials AuthCredentials {
            get {
                if (_authCredentialsCacheNeedReset) {
                    var str = AppSettings.GetValueOrDefault<string>("auth_credentials", null);
                    if (string.IsNullOrWhiteSpace(str)) return null;

                    _authCredentialsCache = AuthCredentials.Deserialize(str);
                    _authCredentialsCacheNeedReset = false;
                }

                return _authCredentialsCache;
            }
            set {
                _authCredentialsCacheNeedReset = true;
                AppSettings.AddOrUpdateValue("auth_credentials", value?.Serialize());
            }
        }



        public static string EditPersonUrl { get; } = EDIT_PERSON_URL;
        public static string EditPersonUrlAdmin { get; } = EDIT_PERSON_URL_ADMIN;

    }

}