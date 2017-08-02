using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public class AddPageAdminViewModel : BaseAddViewModel {

        public AddPageAdminViewModel() : base() { }

        public class User {
            public int Id { get; set; }
            public string Fio { get; set; }
            public string Email { get; set; }
        }
        private User _owner;
        public User Owner {
            get => _owner; 
            set {
                if (_owner != value) {
                    _owner = value;
                    this.OnPropertyChanged(nameof(OwnerText));
                }
            }
        }
        public string OwnerText { get => this.Owner == null ? "Не выбрано" : $"{this.Owner.Id}: {this.Owner.Email}"; }

        public ICommand SelectOwnerCommand {
            get => new Command(async () => {
                var page = new UserListPage();
                page.ItemSelected += async (sender, user) => {
                    Owner = new User { Id = user.Id, Email = user.Fio, Fio = user.Fio };
                    await App.GlobalPage.Pop();
                };
                await App.GlobalPage.Push(page);
            });
        }

        protected override async void Send() {
            var errors = new List<string>();
            if (this.Owner == null) errors.Add("Не указан владелец");
            if (this.IsDeadPerson && !this.UserPosition.HasValue) errors.Add("Ошибка определения местоположения");
            if (String.IsNullOrWhiteSpace(this.FirstName)) errors.Add("Поле \"Имя\" не заполнено");

            if (errors.Count() > 0) {
                Device.BeginInvokeOnMainThread(() => App.Current.MainPage.DisplayAlert("Ошибка", string.Join("\n", errors), "ОК"));
                return;
            }

            App.ToastNotificator.Show("Отправка на сервер");

            try {
                using (var handler = new HttpClientHandler { CookieContainer = AuthHelper.CookieContainer })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(20) })
                using (var content = new MultipartFormDataContent(String.Format("----------{0:N}", Guid.NewGuid()))) {
                    content.Add(new StringContent(this.Owner.Id.ToString()), "owner");
                    content.Add(new StringContent(this.IsDeadPerson ? "dead" : "alive"), "type");
                    if (!string.IsNullOrWhiteSpace(this.FirstName))
                        content.Add(new StringContent(this.FirstName), "first_name");
                    if (!string.IsNullOrWhiteSpace(this.SecondName))
                        content.Add(new StringContent(this.SecondName), "second_name");
                    if (!string.IsNullOrWhiteSpace(this.LastName))
                        content.Add(new StringContent(this.LastName), "last_name");
                    if (this.DateBirth.HasValue)
                        content.Add(new StringContent(this.DateBirth.Value.ToString("yyyy-MM-dd")), "date_birth");
                    if (this.PhotoSource != null && this.PhotoSource is FileImageSource) {
                        var result = DependencyService.Get<IFileStreamPicker>().GetStream((PhotoSource as FileImageSource).File);
                        content.Add(new StreamContent(result), "photo", "photo.jpg");
                    }

                    if (this.IsDeadPerson) {
                        if (this.UserPosition.HasValue) {
                            content.Add(new StringContent(this.UserPosition.Value.Latitude.ToString(CultureInfo.InvariantCulture)), "latitude");
                            content.Add(new StringContent(this.UserPosition.Value.Longitude.ToString(CultureInfo.InvariantCulture)), "longitude");
                        }
                        if (this.DateDeath.HasValue)
                            content.Add(new StringContent(this.DateDeath.Value.ToString("yyyy-MM-dd")), "date_death");
                        if (!string.IsNullOrWhiteSpace(this.Text))
                            content.Add(new StringContent(this.Text), "text");
                        if (this.Height.HasValue)
                            content.Add(new StringContent(this.Height.Value.ToString(CultureInfo.InvariantCulture)), "height");
                        if (this.Width.HasValue)
                            content.Add(new StringContent(this.Width.Value.ToString(CultureInfo.InvariantCulture)), "width");
                        if (this.SchemeStream != null) {
                            content.Add(new StreamContent(this.SchemeStream), "scheme", this.SchemeName);
                        }

                    }

                    using (var response = await client.PostAsync(Settings.ADD_PERSON_URL_ADMIN, content)) {
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                            await AuthHelper.ReloginAsync();
                            return;
                        }

                        response.EnsureSuccessStatusCode();
                        var str = await response.Content.ReadAsStringAsync();
                        var json = Newtonsoft.Json.JsonConvert.DeserializeObject<PersonJson.Update>(str);
                        var person = Person.ConvertFromJson(json);
                        await App.Database.AddPerson(person);

                        App.ToastNotificator.Show("Данные успешно отправлены");

                        var page = new EditPersonPageAdmin(person.Id);
                        await App.GlobalPage.ReplaceCurrentPage(page);
                    }
                }
            } catch {
                Device.BeginInvokeOnMainThread(() =>
                    App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка отправки данных", "ОК"));
            }
        }
    }
}
