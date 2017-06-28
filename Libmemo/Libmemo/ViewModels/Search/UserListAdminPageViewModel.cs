using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo {
    class UserListAdminPageViewModel : BaseSearchViewModel<UserListAdminPageViewModel.User> {
        public class User : ISearchFiltrable {
            public string FilterString => Email;
            public string ShowString => $"{Id}: {Email}";

            public int Id { get; set; }
            public string Email { get; set; }
            public string Fio { get; set; }
        }

        public UserListAdminPageViewModel() : base() {
            Task.Run(async () => await Load());
            this.ItemSelected += (object sender, User item) => {
                var q = 1;
            };
        }


        class JsonData {
            public int id { get; set; }
            public string fio { get; set; }
            public string email { get; set; }
        }
        private async Task Load() {
            try {
                using (var handler = new HttpClientHandler { CookieContainer = AuthHelper.CookieContainer })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) })
                using (var responce = await client.GetAsync(Settings.USER_LIST_URL)) {

                    if (responce.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        await AuthHelper.ReloginAsync();
                        return;
                    }

                    responce.EnsureSuccessStatusCode();
                    var str = await responce.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<List<JsonData>>(str);

                    this.Data = data.Select(i => new User { Id = i.id, Fio = i.fio, Email = i.email });
                }
            } catch (Exception) {
                App.ToastNotificator.Show("Ошибка загрузки данных");
            }
        }
    }




}
