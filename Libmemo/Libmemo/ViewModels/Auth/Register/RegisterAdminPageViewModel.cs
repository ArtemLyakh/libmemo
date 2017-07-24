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

        public RegisterAdminPageViewModel() { }

        public class Json {
            public class User {
                public int id { get; set; }
                public bool is_admin { get; set; }
                public string fio { get; set; }
                public string email { get; set; }
            }

            public User user { get; set; }
            public PersonJson.Update person { get; set; }
        }

        public ICommand BackCommand => new Command(async () => await App.GlobalPage.Pop());

        private bool IsBusy { get; set; } = false;
        public ICommand RegisterCommand => new Command(async () => {
            if (IsBusy) return;

            var errors = this.Validate();
            if (errors.Count() > 0) {
                App.ToastNotificator.Show(string.Join("\n", errors));
                return;
            }

            App.ToastNotificator.Show("Регистрация");
            IsBusy = true;

            try {
                using (var handler = new HttpClientHandler { CookieContainer = AuthHelper.CookieContainer })
                using (var request = new HttpRequestMessage(HttpMethod.Post, Settings.REGISTER_URL_ADMIN) {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                        {"email", this.Email },
                        {"password", this.Password },
                        {"confirm", this.ConfirmPassword }
                    })
                })
                using (HttpClient client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(3) })
                using (var responce = await client.SendAsync(request)) {
                    var str = await responce.Content.ReadAsStringAsync();

                    if (responce.StatusCode == HttpStatusCode.Unauthorized)
                        throw new UnauthorizedAccessException();

                    if (responce.StatusCode == HttpStatusCode.BadRequest) {
                        var error = JsonConvert.DeserializeObject<JsonMessage>(str).message;
                        throw new HttpRequestException(error);
                    }

                    responce.EnsureSuccessStatusCode();

                    var json = JsonConvert.DeserializeObject<Json>(str);

                    var person = Person.ConvertFromJson(json.person);
                    await App.Database.AddPerson(person);

                    App.ToastNotificator.Show("Пользователь зарегистрирован");

                    await App.GlobalPage.ReplaceCurrentPage(new PersonalDataPageAdmin(json.user.id));
                }
            } catch (UnauthorizedAccessException) {
                await AuthHelper.ReloginAsync();
            } catch (TaskCanceledException) {
                App.ToastNotificator.Show("Превышен таймаут запроса");
            } catch (HttpRequestException ex) {
                App.ToastNotificator.Show(ex.Message);
            } catch {
                App.ToastNotificator.Show("Ошибка");
            } finally {
                IsBusy = false;
            }
       });
    }
}
