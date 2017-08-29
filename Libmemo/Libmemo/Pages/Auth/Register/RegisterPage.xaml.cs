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
            
            public ICommand RegisterCommand => new Command(async () => {
                if (cancelTokenSource != null) return;

                var errors = this.Validate();
                if (errors.Count() > 0) {
                    App.ToastNotificator.Show(string.Join("\n", errors));
                    return;
                }

                StartLoading("Регистрация");
                var uri = new Uri(Settings.REGISTER_URL);
                var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                    {"email", this.Email },
                    {"password", this.Password },
                    {"confirm", this.ConfirmPassword }
                });

                HttpResponseMessage response = null;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Post, uri, content, 20, cancelTokenSource.Token);
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
                    var str = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.BadRequest) {
                        var error = JsonConvert.DeserializeObject<Json.Message>(str).message;
                        throw new HttpRequestException(error);
                    }

                    response.EnsureSuccessStatusCode();

                    var json = JsonConvert.DeserializeObject<Json.Register>(str);
                    var authInfo = new AuthInfo(
                        UserId: json.auth.id,
                        IsAdmin: json.auth.is_admin,
                        Email: json.auth.email,
                        Fio: json.auth.fio,
                        CookieContainer: Settings.Cookies
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

            });

            public ICommand LoginCommand => new Command(async () => await App.GlobalPage.PushRoot(new Pages.Login()));

        }
    }
}
