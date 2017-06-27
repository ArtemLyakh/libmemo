using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo {
    public class AuthInfo {

        public AuthInfo(bool IsAdmin, int UserId, string Email, string Fio, CookieContainer CookieContainer) {
            this.IsAdmin = IsAdmin;
            this.UserId = UserId;
            this.Email = Email;
            this.Fio = Fio;
            this.CookieContainer = CookieContainer;
        }

        public bool IsAdmin { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Fio { get; set; }
        public CookieContainer CookieContainer { get; set; }

        class Serializable {
            public string is_admin { get; set; }
            public string user_id { get; set; }
            public string email { get; set; }
            public string fio { get; set; }
            public string cookie_container { get; set; }
        }

        public string Serialize() {
            var serializable = new Serializable() {
                is_admin = IsAdmin.ToString(),
                user_id = UserId.ToString(),
                email = Email,
                fio = Fio
            };

            if (CookieContainer != null) {
                List<Cookie> cookieList = new List<Cookie>();
                foreach (var item in CookieContainer.GetCookies(new Uri(Settings.SERVER_URL))) {
                    var cookie = (Cookie)item;
                    cookieList.Add(cookie);
                }
                if (cookieList.Count > 0) {
                    serializable.cookie_container = JsonConvert.SerializeObject(cookieList);
                }
            }

            return JsonConvert.SerializeObject(serializable);
        }

        public static AuthInfo Deserialize(string serialized) {
            var deserialized = JsonConvert.DeserializeObject<Serializable>(serialized);

            CookieContainer cookieContainer = null;
            if (!string.IsNullOrWhiteSpace(deserialized.cookie_container)) {
                List<Cookie> cookieList = JsonConvert.DeserializeObject<List<Cookie>>(deserialized.cookie_container);

                CookieCollection cookieCollection = new CookieCollection();
                foreach (var cookie in cookieList) {
                    cookieCollection.Add(cookie);
                }

                cookieContainer = new CookieContainer();
                cookieContainer.Add(new Uri(Settings.SERVER_URL), cookieCollection);
            }

            return new AuthInfo(
                IsAdmin: bool.Parse(deserialized.is_admin),
                UserId: int.Parse(deserialized.user_id),
                Fio: deserialized.fio,
                Email: deserialized.email,
                CookieContainer: cookieContainer
            );
        }


    }
}
