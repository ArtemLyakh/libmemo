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
    public partial class RegisterPage : ContentPage {

        private ViewModel Model {
            get => (ViewModel)BindingContext;
            set => BindingContext = value;
        }

        public RegisterPage() {
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
                HttpClientHandler handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
                try {
                    App.ToastNotificator.Show("Регистрация");
                    timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    cancelToken = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken.Token);

                    using (var request = new HttpRequestMessage(HttpMethod.Post, Settings.REGISTER_URL) {
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
                        var str = await responce.Content.ReadAsStringAsync();

                        if (responce.StatusCode == HttpStatusCode.BadRequest) {
                            var error = JsonConvert.DeserializeObject<Json.Message>(str).message;
                            throw new HttpRequestException(error);
                        }

                        responce.EnsureSuccessStatusCode();

                        var json = JsonConvert.DeserializeObject<Json.Register>(str);
                        var authInfo = new AuthInfo(
                            UserId: json.auth.id,
                            IsAdmin: json.auth.is_admin,
                            Email: json.auth.email,
                            Fio: json.auth.fio,
                            CookieContainer: handler.CookieContainer
                        );
                        var authCredentials = new AuthCredentials(
                            Email: Email,
                            Password: Password
                        );

                        var person = Person.ConvertFromJson(json.person);
                        await App.Database.AddPerson(person);

                        AuthHelper.Login(authInfo, authCredentials);

                        await App.GlobalPage.PopToRootPage();
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
