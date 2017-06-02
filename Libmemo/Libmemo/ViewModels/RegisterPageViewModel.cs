using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public class RegisterPageViewModel : INotifyPropertyChanged {

        private string _email;
        public string Email {
            get { return this._email; }
            set {
                if (this._email != value) {
                    this._email = value;
                    this.OnPropertyChanged(nameof(Email));
                }
            }
        }

        private string _password;
        public string Password {
            get { return this._password; }
            set {
                if (this._password != value) {
                    this._password = value;
                    this.OnPropertyChanged(nameof(Password));
                }
            }
        }

        private string _confirmPassword;
        public string ConfirmPassword {
            get { return this._confirmPassword; }
            set {
                if (this._confirmPassword != value) {
                    this._confirmPassword = value;
                    this.OnPropertyChanged(nameof(ConfirmPassword));
                }
            }
        }


        public ICommand LoginCommand {
            get {
                return new Command(() => {
                    App.MenuPage.ExecuteMenuItem(MenuItemId.Login);
                });
            }
        }

        public ICommand RegisterCommand {
            get {
                return new Command(async () => {

                    if (!Regex.IsMatch(this.Email,
                            @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                            RegexOptions.IgnoreCase)) { 
                        App.ToastNotificator.Show("Невалидный email адрес");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(this.Password)) {
                        App.ToastNotificator.Show("Не заполнен пароль");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(this.ConfirmPassword)) {
                        App.ToastNotificator.Show("Не заполнено подтверждение пароля");
                        return;
                    }
                    if (!this.Password.Equals(this.ConfirmPassword)) {
                        App.ToastNotificator.Show("Пароли не совпадают");
                        return;
                    }


                    try {
                        using (var handler = new HttpClientHandler { CookieContainer = new CookieContainer() })
                        using (var request = new HttpRequestMessage(HttpMethod.Post, Settings.RegisterUri) {
                            Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                                {"email", this.Email },
                                {"password", this.Password },
                                {"confirm", this.ConfirmPassword }
                            })
                        })
                        using (HttpClient client = new HttpClient(handler) { Timeout = new TimeSpan(0, 0, 5) })
                        using (var responce = await client.SendAsync(request)) {
                            var str = await responce.Content.ReadAsStringAsync();
                            if (responce.StatusCode == HttpStatusCode.BadRequest) {
                                var error = JsonConvert.DeserializeObject<JsonMessage>(str).message;
                                App.ToastNotificator.Show(error);
                                return;
                            }
                            responce.EnsureSuccessStatusCode();

                            App.ToastNotificator.Show("Регистрация успешно завершена");

                            AuthHelper.Login(
                                this.Email,
                                this.Password,
                                "default",
                                handler.CookieContainer
                            );

                            App.MenuPage.ExecuteMenuItem(MenuItemId.Edit);
                        }
                    } catch {
                        App.ToastNotificator.Show("Ошибка регистрации");
                    }
                });
            }
        }




        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
