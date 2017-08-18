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

        public class ViewModel : BaseTreeViewModel {

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

                HttpResponseMessage response = null;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Post, new Uri(Settings.TREE_DATA_URL), content, 20, cancelTokenSource.Token);
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
                if (response == null) return;

                try {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }
                    response.EnsureSuccessStatusCode();
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

                HttpResponseMessage response = null;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Get, new Uri(Settings.TREE_DATA_URL), null, 10, cancelTokenSource.Token);
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
                if (response == null) return;

                Tree.Json json = null;
                try {
                    var str = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }
                    response.EnsureSuccessStatusCode();

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