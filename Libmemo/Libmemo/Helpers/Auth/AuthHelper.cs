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
                case "admin": Settings.UserType = UserType.Admin; break;
                default: Settings.UserType = UserType.None; break;
            }
            Settings.Email = email;
            Settings.Password = password;
            Settings.Logged = true;

            App.MenuPage.SetMenuPage();

            App.ToastNotificator.Show($"Добро пожаловать, {email}");
        }

        private static void InnerLogout() {
            Settings.Logged = false;
            Settings.UserType = UserType.None;
            Settings.AuthCookies = null;

            App.MenuPage.SetMenuPage();
        }

        public static void Logout() {
            InnerLogout();

            //принудительный сброс страницы
            App.MenuPage.ExecuteMenuItem("Карта");
        }

        public static void Relogin() {
            InnerLogout();

            App.MenuPage.ExecuteMenuItem("Авторизация");
        }

        public static bool IsLogged() {
            return Settings.Logged;
        }
    }


    public enum UserType {
        None = 0, Default, Admin
    }
    public class JsonMessage {
        public string message { get; set; }
    }

}
