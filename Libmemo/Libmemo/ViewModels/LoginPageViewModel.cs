using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public class LoginPageViewModel : INotifyPropertyChanged {
        public LoginPageViewModel() {

        }

        private string _email = AuthHelper.UserEmail;
        public string Email {
            get { return this._email; }
            set {
                if (this._email != value) {
                    this._email = value;
                    this.OnPropertyChanged(nameof(Email));
                }
            }
        }

        private string _password = AuthHelper.UserPassword;
        public string Password {
            get { return this._password; }
            set {
                if (this._password != value) {
                    this._password = value;
                    this.OnPropertyChanged(nameof(Password));
                }
            }
        }

        public ICommand LoginCommand {
            get => new Command(async () => {
                if (string.IsNullOrWhiteSpace(this.Email)) {
                    App.ToastNotificator.Show("Не заполнен email");
                    return;
                }
                if (string.IsNullOrWhiteSpace(this.Password)) {
                    App.ToastNotificator.Show("Не заполнен пароль");
                    return;
                }

                try {
                    using (var handler = new HttpClientHandler { CookieContainer = new CookieContainer() })
                    using (var request = new HttpRequestMessage(HttpMethod.Post, Settings.LOGIN_URL) {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                            {"email", this.Email },
                            {"password", this.Password }
                        })
                    })
                    using (HttpClient client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) })
                    using (var responce = await client.SendAsync(request)) {
                        var str = await responce.Content.ReadAsStringAsync();
                        if (responce.StatusCode == HttpStatusCode.BadRequest) {
                            JsonSerializerSettings settings = new JsonSerializerSettings();
                            var error = JsonConvert.DeserializeObject<JsonMessage>(str).message;
                            App.ToastNotificator.Show(error);
                            return;
                        }
                        responce.EnsureSuccessStatusCode();


                        var authJson = JsonConvert.DeserializeObject<AuthJson>(str);
                        var authInfo = new AuthInfo(
                            UserId: authJson.id,
                            PersonId: authJson.person_id,
                            IsAdmin: authJson.is_admin,
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
                    }
                } catch {
                    App.ToastNotificator.Show("Ошибка авторизации");
                }
            });
        }

        public ICommand RegisterCommand {
            get => new Command(async () => await App.GlobalPage.PushRoot(new RegisterPage()));
        }




        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }





}
