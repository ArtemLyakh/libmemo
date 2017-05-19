using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestAuthPage : ContentPage {
        public TestAuthPage() {
            InitializeComponent();

            this.ToolbarItems.Add(new ToolbarItem("test", "default_pin.png", () => {
                var q = 1;
            }));
        }

        private async void Button_Clicked(object sender, EventArgs e) {

            string loginUri = "http://libmemo.com/api/login.php";
            string email = "test@test.test";
            string password = "123321";

            using (var handler = new HttpClientHandler { CookieContainer = new CookieContainer() })
            using (var request = new HttpRequestMessage(HttpMethod.Post, loginUri)) {
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                    {"email", email },
                    {"password", password }
                });
                using (HttpClient client = new HttpClient(handler))
                using (var responce = await client.SendAsync(request)) {
                    var str = await responce.Content.ReadAsStringAsync();
                    Settings.AuthCookies = handler.CookieContainer;
                }
            }

        }

        private async void Button_Clicked_1(object sender, EventArgs e) {

            using (var handler = new HttpClientHandler { CookieContainer = Settings.AuthCookies })
            using (var client = new HttpClient(handler))
            using (var res = await client.GetAsync("http://libmemo.com/api/getauth.php")) {
                var str = await res.Content.ReadAsStringAsync();
            }

        }
    }
}
