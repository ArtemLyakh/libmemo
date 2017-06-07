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
    public class RegisterAdminPageViewModel : BaseRegisterViewModel {

        public ICommand RegisterCommand {
            get => new Command(async () => {

                var errors = this.Validate();
                if (errors.Count > 0) {
                    App.ToastNotificator.Show(string.Join("\n", errors));
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

                        //TODO добавить переход на редактирование пользователя от админа
                        App.MenuPage.ExecuteMenuItem(MenuItemId.Map);
                    }
                } catch {
                    App.ToastNotificator.Show("Ошибка регистрации");
                }
            });

        }


    }
}
