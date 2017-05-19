using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Newtonsoft.Json;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestAuthPage : ContentPage {
        public TestAuthPage() {
            InitializeComponent();

        }

        private class JsonSecret {
            public string secret { get; set; }
        }
        private async void Button_Clicked_1(object sender, EventArgs e) {
            var prev = Settings.AuthCookies;
            try {
                using (var handler = new HttpClientHandler { CookieContainer = Settings.AuthCookies })
                using (var client = new HttpClient(handler))
                using (var res = await client.GetAsync("http://libmemo.com/api/getauth.php")) {
                    res.EnsureSuccessStatusCode();
                    var str = await res.Content.ReadAsStringAsync();
                    var secret = JsonConvert.DeserializeObject<JsonSecret>(str);
                    App.ToastNotificator.Show($"Secret: {secret.secret}");
                    var midd = handler.CookieContainer;
                    var qwe = 1;
                }
            } catch {
                App.ToastNotificator.Show("Error");
            }

            var neww = Settings.AuthCookies;
            var q = 1;
        }
    }
}
