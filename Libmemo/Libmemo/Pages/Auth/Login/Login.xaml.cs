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

namespace Libmemo.Pages {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage {

        private ViewModel Model {
            get => (ViewModel)BindingContext;
            set => BindingContext = value;
        }

        public Login() {
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



            public ICommand LoginCommand => new Command(async () => {
                if (cancelTokenSource != null) return;

                if (string.IsNullOrWhiteSpace(this.Email)) {
                    App.ToastNotificator.Show("Не заполнен email");
                    return;
                }
                if (string.IsNullOrWhiteSpace(this.Password)) {
                    App.ToastNotificator.Show("Не заполнен пароль");
                    return;
                }

                StartLoading("Авторизация");


                var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                    {"email", this.Email },
                    {"password", this.Password }
                });

                
                HttpResponseMessage response;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Post, new Uri(Settings.LOGIN_URL), content, 30, cancelTokenSource.Token);
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


                var str = await response.Content.ReadAsStringAsync();


				try
				{
					if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
					{
						throw new UnauthorizedAccessException();
					}
					if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) {
						var msg = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Error>(str);
						throw new HttpRequestException(msg.error);
					}
					response.EnsureSuccessStatusCode();
				}
				catch (UnauthorizedAccessException)
				{
					await AuthHelper.ReloginAsync();
					return;
				}
				catch (HttpRequestException ex)
				{
					App.ToastNotificator.Show(ex.Message);
					return;
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка");
					return;
				}



				var authJson = JsonConvert.DeserializeObject<Json.Auth>(str);
				var authInfo = new AuthInfo(
					IsAdmin: authJson.is_admin,
					UserId: authJson.id,
					Email: authJson.email,
					Fio: authJson.fio,
					CookieContainer: Settings.Cookies
				);
				var authCredentials = new AuthCredentials(
					Email: Email,
					Password: Password
				);

				AuthHelper.Login(authInfo, authCredentials);
                await App.GlobalPage.PopToRootPage();

            });

            public ICommand RegisterCommand => new Command(async () => await App.GlobalPage.PushRoot(new Pages.Register()));


        }

    }
}
