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

        private string _email = Settings.Email;
        public string Email {
            get { return this._email; }
            set {
                if (this._email != value) {
                    this._email = value;
                    this.OnPropertyChanged(nameof(Email));
                }
            }
        }

        private string _password = Settings.Password;
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
            get {
                return new Command(async () => {
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
                        using (var request = new HttpRequestMessage(HttpMethod.Post, Settings.LoginUri) {
                            Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                                {"email", this.Email },
                                {"password", this.Password }
                            })
                        })
                        using (HttpClient client = new HttpClient(handler) { Timeout = new TimeSpan(0, 0, 5) })
                        using (var responce = await client.SendAsync(request)) {
                            var str = await responce.Content.ReadAsStringAsync();
                            if (responce.StatusCode == HttpStatusCode.BadRequest) {
                                JsonSerializerSettings settings = new JsonSerializerSettings();
                                var error = JsonConvert.DeserializeObject<JsonMessage>(str).message;
                                App.ToastNotificator.Show(error);
                                return;
                            }
                            responce.EnsureSuccessStatusCode();

                            AuthHelper.Login(
                                this.Email,
                                this.Password,
                                JsonConvert.DeserializeObject<JsonMessage>(str).message,
                                handler.CookieContainer
                            );

                            App.MenuPage.ExecuteMenuItem("Карта");
                        }
                    } catch {
                        App.ToastNotificator.Show("Ошибка авторизации");
                    }
                });
            }
        }

        public ICommand RegisterCommand {
            get {
                return new Command(() => {
                    App.MenuPage.ExecuteMenuItem("Регистрация");
                });
            }
        }




        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
