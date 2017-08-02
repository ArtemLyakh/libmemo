using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public class UserListPageViewModel : BaseListViewModel<ListElement.TextElement>{

        public UserListPageViewModel() : base() { }

        public ICommand BackCommand => new Command(async () => await App.GlobalPage.Pop());

        public ICommand LoadCommand => new Command(async () => await Load());

        public ICommand AddCommand => new Command(async () => await App.GlobalPage.Push(new RegisterAdminPage()));


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
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JsonData>>(str);

                    this.Data = data.Select(i => new ListElement.TextElement { Id = i.id, Fio = i.fio, Email = i.email }).ToList();
                }
            } catch (Exception) {
                App.ToastNotificator.Show("Ошибка загрузки данных");
            }
        }


    }
}
