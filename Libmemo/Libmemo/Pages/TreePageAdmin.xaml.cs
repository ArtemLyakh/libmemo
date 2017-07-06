using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TreePageAdmin : ContentPage {

        private int UserId { get; set; }
        private Tree Tree { get; set; }

        public TreePageAdmin(int id) {
            InitializeComponent();
            UserId = id;
            Init();
        }


        private async Task<Tree.Json> LoadData() {
            var builder = new UriBuilder(Settings.TREE_DATA_URL_ADMIN);
            builder.Query = $"id={UserId}";
            var uri = builder.Uri;

            using (var handler = new HttpClientHandler { CookieContainer = AuthHelper.CookieContainer })
            using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) })
            using (var responce = await client.GetAsync(uri)) {
                var str = await responce.Content.ReadAsStringAsync();
                if (responce.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                    throw new UnauthorizedAccessException();
                }
                responce.EnsureSuccessStatusCode();

                var json = Newtonsoft.Json.JsonConvert.DeserializeObject<Tree.Json>(str);
                return json;
            }
        }

        private async void Init() {
            if (!AuthHelper.IsAdmin) {
                await AuthHelper.ReloginAsync();
                return;
            }

            int id = UserId;
            Tree = new Tree(id, absolute, scroll);

            try {
                var data = await LoadData();
                await Tree.LoadFromJson(data);
            } catch (UnauthorizedAccessException) {
                await AuthHelper.ReloginAsync();
                return;
            } catch {
                Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка построения дерева", "Ок"));
                await App.GlobalPage.PopToRootPage();
                return;
            }

            Tree.DrawTree();
        }


        private async Task Reset_Button_Clicked(object sender, EventArgs e) {
            try {
                var data = await LoadData();
                await Tree.LoadFromJson(data);
            } catch (UnauthorizedAccessException) {
                await AuthHelper.ReloginAsync();
                return;
            } catch {
                Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка построения дерева", "Ок"));
                await App.GlobalPage.PopToRootPage();
                return;
            }

            Tree.DrawTree();
        }

        private async Task Save_Button_Clicked(object sender, EventArgs e) {
            var data = Tree.GetTreeAsJson();
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            try {
                using (var handler = new HttpClientHandler { CookieContainer = AuthHelper.CookieContainer })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(20) }) {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    using (var response = await client.PostAsync(Settings.TREE_DATA_URL_ADMIN, content)) {
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                            await AuthHelper.ReloginAsync();
                            return;
                        }

                        response.EnsureSuccessStatusCode();

                        App.ToastNotificator.Show("Сохранено");
                    }
                }
            } catch {
                Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка сохранения дерева", "Ок"));
            }
        }


        private async Task ButtonPlus_Clicked(object sender, EventArgs e) => await Tree.ZoomIn();
        private async Task ButtonMinus_Clicked(object sender, EventArgs e) => await Tree.ZoomOut();


    }
}
