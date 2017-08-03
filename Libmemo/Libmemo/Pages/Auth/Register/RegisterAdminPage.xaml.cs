using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterAdminPage : ContentPage {

        private ViewModel Model {
            get => (ViewModel)BindingContext;
            set => BindingContext = value;
        }

        public RegisterAdminPage() {
            InitializeComponent();
            BindingContext = new ViewModel();
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            Model.OnAppearing();
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            Model.OnDisappearing();
        }

        public class ViewModel : BaseRegisterViewModel {

            public ViewModel() : base() { }

            public override void OnDisappearing() {
                base.OnDisappearing();
                cancelToken?.Cancel();
            }


            private CancellationTokenSource cancelToken { get; set; }
            private CancellationTokenSource timeoutToken { get; set; }
            public ICommand RegisterCommand => new Command(async () => {
                if (cancelToken != null) return;

                var errors = this.Validate();
                if (errors.Count() > 0) {
                    App.ToastNotificator.Show(string.Join("\n", errors));
                    return;
                }

                HttpResponseMessage responce = null;
                try {
                    App.ToastNotificator.Show("Регистрация");
                    timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    cancelToken = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken.Token);

                    using (var handler = new HttpClientHandler { CookieContainer = AuthHelper.CookieContainer })
                    using (var request = new HttpRequestMessage(HttpMethod.Post, Settings.REGISTER_URL_ADMIN) {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                            {"email", this.Email },
                            {"password", this.Password },
                            {"confirm", this.ConfirmPassword }
                        })
                    })
                    using (HttpClient client = new HttpClient(handler)) {
                        responce = await client.SendAsync(request, cancelToken.Token);
                    }
                } catch (OperationCanceledException) {
                    if (timeoutToken.Token.IsCancellationRequested) App.ToastNotificator.Show("Превышен интервал запроса");
                    return;
                } finally {
                    cancelToken = null;
                    timeoutToken = null;
                }

                if (responce != null) {
                    try {
                        if (responce.StatusCode == HttpStatusCode.Unauthorized)
                            throw new UnauthorizedAccessException();

                        var str = await responce.Content.ReadAsStringAsync();

                        if (responce.StatusCode == HttpStatusCode.BadRequest) {
                            var error = JsonConvert.DeserializeObject<Json.Message>(str).message;
                            throw new HttpRequestException(error);
                        }

                        responce.EnsureSuccessStatusCode();

                        var json = JsonConvert.DeserializeObject<Json.Register>(str);

                        var person = Person.ConvertFromJson(json.person);
                        await App.Database.AddPerson(person);

                        App.ToastNotificator.Show("Пользователь зарегистрирован");

                        await App.GlobalPage.Pop();
                    } catch (UnauthorizedAccessException) {
                        await AuthHelper.ReloginAsync();
                    } catch (HttpRequestException ex) {
                        App.ToastNotificator.Show(ex.Message);
                    } catch {
                        App.ToastNotificator.Show("Ошибка");
                    }
                }
            });

            public ICommand LoginCommand => new Command(async () => await App.GlobalPage.PushRoot(new LoginPage()));

        }
    }
}
