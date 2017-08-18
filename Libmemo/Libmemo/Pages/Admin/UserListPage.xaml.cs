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
    public partial class UserListPage : ContentPage {
        private ViewModel Model {
            get => (ViewModel)BindingContext;
            set => BindingContext = value;
        }

        public UserListPage() {
            InitializeComponent();
            BindingContext = new ViewModel((sender, e) => this.ItemSelected?.Invoke(this, e));
        }

        public event EventHandler<ListElement.TextElement> ItemSelected;


        protected override void OnAppearing() {
            base.OnAppearing();
            Model.OnAppearing();
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            Model.OnDisappearing();
        }


        public class ViewModel : BaseListViewModel<ListElement.TextElement> {

            public ViewModel(EventHandler<ListElement.TextElement> onItemSelected) : base() {
                ItemSelected += onItemSelected;
            }

            public override void OnAppearing() {
                base.OnAppearing();
                ResetCommand.Execute(null);
            }

            public ICommand AddCommand => new Command(async () => await App.GlobalPage.Push(new RegisterAdminPage()));

            public ICommand ResetCommand => new Command(async () => {
                if (cancelTokenSource != null) return;

                StartLoading("Получение данных");

                HttpResponseMessage responce = null;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    responce = await WebClient.Instance.SendAsync(HttpMethod.Get, new Uri(Settings.USER_LIST_URL), null, 15, cancelTokenSource.Token);
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

                var str = await responce.Content.ReadAsStringAsync();
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Json.User>>(str);

                this.Data = data.Select(i => new ListElement.TextElement { Id = i.id, Fio = i.fio, Email = i.email }).ToList();
            });
        }
    }
}