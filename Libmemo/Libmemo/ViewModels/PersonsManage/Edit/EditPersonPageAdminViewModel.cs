using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo {
    class EditPersonPageAdminViewModel : BaseEditPersonViewModel {

        public EditPersonPageAdminViewModel(int id) : base(id) { }

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

        public ICommand SelectOwnerCommand => new Command(async () => {
            var page = new UserListPage();
            page.ItemSelected += async (sender, user) => {
                Owner = new User { Id = user.Id, Email = user.Fio, Fio = user.Fio };
                await App.GlobalPage.Pop();
            };
            await App.GlobalPage.Push(page);
        });

        class JsonData {
            public int id { get; set; }
            public string fio { get; set; }
            public string email { get; set; }
        }
        protected async override void Reset() {
            base.Reset();
            var person = await App.Database.GetById(Id);

            var builder = new UriBuilder(Settings.USER_DATA_URL);
            builder.Query = $"id={person.Owner}";
            var uri = builder.Uri;

            try {
                using (var handler = new HttpClientHandler { CookieContainer = AuthHelper.CookieContainer })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(3) })
                using (var response = await client.GetAsync(uri)) {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        await AuthHelper.ReloginAsync();
                        return;
                    }

                    response.EnsureSuccessStatusCode();
                    var str = await response.Content.ReadAsStringAsync();
                    var json = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonData>(str);
                    Owner = new User { Id = json.id, Fio = json.fio, Email = json.fio };
                }
            } catch {
                Device.BeginInvokeOnMainThread(() =>
                    App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка загрузки", "ОК"));
            }
        }



        protected override async void Send() {
            var errors = new List<string>();
            if (this.Owner == null) errors.Add("Не указан владелец");
            if (this.IsDeadPerson && this.PersonPosition == default(Position)) errors.Add("Не указано местоположение");
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
                    content.Add(new StringContent(Id.ToString()), "id");
                    content.Add(new StringContent(this.Owner.Id.ToString()), "owner");
                    content.Add(new StringContent(IsDeadPerson ? "dead" : "alive"), "type");

                    content.Add(new StringContent(FirstName), "first_name");
                    content.Add(new StringContent(string.IsNullOrWhiteSpace(SecondName) ? string.Empty : SecondName), "second_name");
                    content.Add(new StringContent(string.IsNullOrWhiteSpace(LastName) ? string.Empty : LastName), "last_name");

                    if (DateBirth.HasValue)
                        content.Add(new StringContent(DateBirth.Value.ToString("yyyy-MM-dd")), "date_birth");
                    if (PhotoSource != null && PhotoSource is FileImageSource) {
                        var result = DependencyService.Get<IFileStreamPicker>().GetStream((PhotoSource as FileImageSource).File);
                        content.Add(new StreamContent(result), "photo", "photo.jpg");
                    }

                    if (IsDeadPerson) {
                        content.Add(new StringContent(PersonPosition.Latitude.ToString(CultureInfo.InvariantCulture)), "latitude");
                        content.Add(new StringContent(PersonPosition.Longitude.ToString(CultureInfo.InvariantCulture)), "longitude");

                        if (DateDeath.HasValue)
                            content.Add(new StringContent(DateDeath.Value.ToString("yyyy-MM-dd")), "date_death");

                        content.Add(new StringContent(string.IsNullOrWhiteSpace(Text) ? string.Empty : Text), "text");

                        if (Height.HasValue)
                            content.Add(new StringContent(Height.Value.ToString(CultureInfo.InvariantCulture)), "height");
                        if (Width.HasValue)
                            content.Add(new StringContent(Width.Value.ToString(CultureInfo.InvariantCulture)), "width");

                        if (SchemeStream != null) {
                            content.Add(new StreamContent(SchemeStream), "scheme", SchemeName);
                        }

                    }

                    using (var response = await client.PostAsync(Settings.EDIT_PERSON_URL_ADMIN, content)) {
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                            await AuthHelper.ReloginAsync();
                            return;
                        }

                        var str = await response.Content.ReadAsStringAsync();
                        response.EnsureSuccessStatusCode();
                        var json = Newtonsoft.Json.JsonConvert.DeserializeObject<PersonJson.Update>(str);
                        var person = Person.ConvertFromJson(json);
                        await App.Database.AddPerson(person);

                        App.ToastNotificator.Show("Данные успешно отправлены");

                        ResetCommand.Execute(null);
                    }
                }
            } catch {
                Device.BeginInvokeOnMainThread(() =>
                    App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка редактирования", "ОК"));
            }

        }

        class JsonDelete {
            public int id { get; set; }
        }
        protected override async void Delete() {
            var confirm = await App.Current.MainPage.DisplayAlert("Удаление", "Вы уверены?", "Да", "Нет");

            if (!confirm) return;

            try {
                using (var handler = new HttpClientHandler { CookieContainer = AuthHelper.CookieContainer })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) })
                using (var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                    { "id", this.Id.ToString() }
                }))
                using (var response = await client.PostAsync(Settings.DELETE_PERSON_URL_ADMIN, content)) {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        await AuthHelper.ReloginAsync();
                        return;
                    }
                    var str = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    var json = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonDelete>(str);

                    await App.Database.DeletePerson(json.id);

                    await App.GlobalPage.Pop();
                }
            } catch {
                await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка удаления", "ОК");
            }


        }

    }


}

