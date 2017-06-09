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
                case "admin": Settings.CurrentUser = -1; break;
                default: Settings.CurrentUser = int.TryParse(type, out int userId) ? userId : default(int); break;
            }
            Settings.Email = email;
            Settings.Password = password;

            App.InitMenu();

            App.ToastNotificator.Show($"Добро пожаловать, {email}");
        }

        private static void InnerLogout() {
            Settings.CurrentUser = default(int);
            Settings.AuthCookies = null;

            App.InitMenu();
        }

        public static async Task Logout() {
            InnerLogout();

            //принудительный сброс страницы
            await App.GlobalPage.PopToRootPage();
        }

        public static async Task ReloginAsync() {
            InnerLogout();

            await App.GlobalPage.PushRoot(new LoginPage());
        }

        public static bool IsLogged { get => Settings.CurrentUser != default(int); }
        public static bool IsAdmin { get => Settings.CurrentUser == -1; }
        public static int? CurrentUserId { get => Settings.CurrentUser > 0 ? (int?)Settings.CurrentUser : null; }
    }


    public enum UserType {
        None = 0, Default, Admin
    }
    public class JsonMessage {
        public string message { get; set; }
    }

}
