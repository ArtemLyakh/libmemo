using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    public partial class TreePage : ContentPage {
        private ViewModel Model {
            get => (ViewModel)BindingContext;
            set => BindingContext = value;
        }

        public TreePage() {
            InitializeComponent();
            BindingContext = new ViewModel(absolute, scroll);
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            Model.OnAppearing();
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            Model.OnDisappearing();
        }

        public class ViewModel : TreeViewModel {

            public ViewModel(AbsoluteLayout absolute, ScrollView scroll) : base(AuthHelper.CurrentUserId, absolute, scroll)  {
                ResetCommand.Execute(null);
            }

            protected override async void Save() {
                if (cancelTokenSource != null) return;

                var data = Tree?.GetTreeAsJson();
                if (data == null) return;

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                StartLoading("Сохранение");

                HttpResponseMessage responce = null;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    responce = await WebClient.Instance.SendAsync(HttpMethod.Post, new Uri(Settings.TREE_DATA_URL), content, 20, cancelTokenSource.Token);
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
                    StopLoading();
                }
                if (responce == null) return;

                try {
                    if (responce.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }
                    responce.EnsureSuccessStatusCode();
                } catch (UnauthorizedAccessException) {
                    await AuthHelper.ReloginAsync();
                    return;
                } catch {
                    App.ToastNotificator.Show("Ошибка");
                    return;
                }


                App.ToastNotificator.Show("Сохранено");
            }

            protected override async void Reset() {
                if (cancelTokenSource != null) return;

                StartLoading("Получение данных");

                HttpResponseMessage responce = null;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    responce = await WebClient.Instance.SendAsync(HttpMethod.Get, new Uri(Settings.TREE_DATA_URL), null, 10, cancelTokenSource.Token);
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
                    StopLoading();
                }
                if (responce == null) return;

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
            }
        }
    }
}