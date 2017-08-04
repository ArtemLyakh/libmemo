using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo {
    public static class AuthHelper {

        public static void Login(AuthInfo info, AuthCredentials credentials) {
            Settings.AuthInfo = info;
            Settings.AuthCredentials = credentials;

            App.InitMenu();

            App.ToastNotificator.Show($"Добро пожаловать, {(!string.IsNullOrWhiteSpace(info.Fio) ? info.Fio : info.Email)}");
        }

        private static void InnerLogout() {
            Settings.AuthInfo = null;
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

        public static bool IsLogged { get => Settings.AuthInfo != null; }
        public static bool IsAdmin { get => Settings.AuthInfo?.IsAdmin ?? false; }
        public static int? CurrentUserId { get => Settings.AuthInfo?.UserId; }

        public static string UserEmail { get => Settings.AuthCredentials?.Email; }
        public static string UserPassword { get => Settings.AuthCredentials?.Password; }
        public static CookieContainer CookieContainer { get => Settings.Cookies; }
    }



    public class JsonMessage {
        public string message { get; set; }
    }

}
