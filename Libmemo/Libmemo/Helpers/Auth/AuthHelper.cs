using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo {
    public static class AuthHelper {
        public static void Login(string email, string password, string type, System.Net.CookieContainer cookieContainer) {

            Settings.AuthCookies = cookieContainer;
            switch (type) {
                case "default": Settings.UserType = UserType.Default; break;
                case "privilege": Settings.UserType = UserType.Privilege; break;
                default: Settings.UserType = UserType.None; break;
            }
            Settings.Email = email;
            Settings.Password = password;
            Settings.Logged = true;

            App.MenuPage.SetMenuPage();

            App.ToastNotificator.Show($"Добро пожаловать, {email}");
        }

        public static void Logout() {
            Settings.Logged = false;
            Settings.UserType = UserType.None;
            Settings.AuthCookies = null;

            App.MenuPage.SetMenuPage();

            //принудительный сброс страницы
            App.MenuPage.ExecuteMenuItem("Карта");
        }

        public static bool IsLogged() {
            return Settings.Logged;
        }
    }


    public enum UserType {
        None = 0, Default, Privilege
    }
    public class JsonLoginSuccess {
        public string type { get; set; }
    }
    public class JsonLoginFail {
        public string error { get; set; }
    }
    public class JsonRegisterSuccess {
        public string message { get; set; }
    }
    public class JsonRegisterFail {
        public string error { get; set; }
    }


}
