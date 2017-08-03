using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class LoginPage : ContentPage {

        private ViewModel Model {
            get => (ViewModel)BindingContext;
            set => BindingContext = value;
        }

        public LoginPage() {
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

        public class ViewModel : BaseViewModel {

            public ViewModel() : base() { }

            public override void OnDisappearing() {
                base.OnDisappearing();
                cancelToken?.Cancel();
            }

            private string _email = AuthHelper.UserEmail;
            public string Email {
                get => this._email;
                set {
                    if (this._email != value) {
                        this._email = value;
                        this.OnPropertyChanged(nameof(Email));
                    }
                }
            }

            private string _password = AuthHelper.UserPassword;
            public string Password {
                get => this._password;
                set {
                    if (this._password != value) {
                        this._password = value;
                        this.OnPropertyChanged(nameof(Password));
                    }
                }
            }



            private CancellationTokenSource cancelToken { get; set; }
            private CancellationTokenSource timeoutToken { get; set; }
            public ICommand LoginCommand => new Command(async () => {
                if (cancelToken != null) return;

                if (string.IsNullOrWhiteSpace(this.Email)) {
                    App.ToastNotificator.Show("Не заполнен email");
                    return;
                }
                if (string.IsNullOrWhiteSpace(this.Password)) {
                    App.ToastNotificator.Show("Не заполнен пароль");
                    return;
                }

                HttpResponseMessage responce = null;
                HttpClientHandler handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
                try {
                    App.ToastNotificator.Show("Авторизация");
                    timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    cancelToken = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken.Token);

                    using (var request = new HttpRequestMessage(HttpMethod.Post, Settings.LOGIN_URL) {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                        {"email", this.Email },
                        {"password", this.Password }
                    }) })
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
                        var str = await responce.Content.ReadAsStringAsync();

                        if (responce.StatusCode == HttpStatusCode.BadRequest) {
                            var error = JsonConvert.DeserializeObject<Json.Message>(str).message;
                            throw new HttpRequestException(error);
                        }

                        responce.EnsureSuccessStatusCode();

                        var authJson = JsonConvert.DeserializeObject<Json.Auth>(str);
                        var authInfo = new AuthInfo(
                            IsAdmin: authJson.is_admin,
                            UserId: authJson.id,
                            Email: authJson.email,
                            Fio: authJson.fio,
                            CookieContainer: handler.CookieContainer
                        );
                        var authCredentials = new AuthCredentials(
                            Email: Email,
                            Password: Password
                        );

                        AuthHelper.Login(authInfo, authCredentials);

                        await App.GlobalPage.PopToRootPage();
                    } catch (HttpRequestException ex) {
                        App.ToastNotificator.Show(ex.Message);
                    } catch {
                        App.ToastNotificator.Show("Ошибка");
                    }
                }
            });

            public ICommand RegisterCommand => new Command(async () => await App.GlobalPage.PushRoot(new RegisterPage()));


        }

    }
}
