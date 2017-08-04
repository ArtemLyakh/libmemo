using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TreePageAdmin : ContentPage {

        private int UserId { get; set; }
        private Tree Tree { get; set; }

        public TreePageAdmin(int id) {
            InitializeComponent();
            BindingContext = this;
            UserId = id;
            Init();
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            cancelTokenSource?.Cancel();
        }

        private async void Init() {
            if (!AuthHelper.IsAdmin) {
                await AuthHelper.ReloginAsync();
                return;
            }

            int id = UserId;
            Tree = new Tree(id, absolute, scroll);

            ResetCommand.Execute(null);
        }



        public ICommand BackCommand => new Command(async () => await App.GlobalPage.Pop());

        public ICommand SaveCommand => new Command(async () => {
            var data = Tree.GetTreeAsJson();
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            App.ToastNotificator.Show("Сохранение");
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
        });

        private CancellationTokenSource cancelTokenSource { get; set; }
        public ICommand ResetCommand => new Command(async () => {
            if (cancelTokenSource != null) return;

            HttpResponseMessage responce = null;
            try {
                cancelTokenSource = new CancellationTokenSource();
                var builder = new UriBuilder(Settings.TREE_DATA_URL_ADMIN);
                builder.Query = $"id={UserId}";
                var uri = builder.Uri;
                responce = await WebClient.Instance.SendAsync(HttpMethod.Get, uri, null, 10, cancelTokenSource.Token);
            } catch (TimeoutException) {
                App.ToastNotificator.Show("Превышен интервал запроса");
                return;
            } catch (OperationCanceledException) { //cancel
                return;
            } catch {
                App.ToastNotificator.Show("Ошибка");
                return;
            } finally {
                cancelTokenSource = null;
            }

            Tree.Json json = null;
            try {
                var str = await responce.Content.ReadAsStringAsync();

                if (responce.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                    throw new UnauthorizedAccessException();
                }
                responce.EnsureSuccessStatusCode();

                json = Newtonsoft.Json.JsonConvert.DeserializeObject<Tree.Json>(str);
            } catch (UnauthorizedAccessException) {
                await AuthHelper.ReloginAsync();
                return;
            } catch {
                App.ToastNotificator.Show("Ошибка");
                return;
            }

            await Tree.LoadFromJson(json);
            Tree.DrawTree();
        });

        public ICommand ZoomInCommand => new Command(async () => await Tree?.ZoomIn());
        public ICommand ZoomOutCommand => new Command(async () => await Tree?.ZoomOut());

    }
}
