using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo {
    class AddPageViewModel : BaseAddViewModel {

        public AddPageViewModel() : base() { }

        protected override async void Send() {
            var errors = new List<string>();
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
                        var result = await DependencyService.Get<IImageFileToByteArrayConverter>().Get(this.PhotoSource);
                        content.Add(new ByteArrayContent(result), "photo", "photo.jpg");
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

                    using (var response = await client.PostAsync(Settings.ADD_PERSON_URL, content)) {
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                            await AuthHelper.ReloginAsync();
                            return;
                        }

                        response.EnsureSuccessStatusCode();
                        var str = await response.Content.ReadAsStringAsync();
                        var json = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonData.PersonJsonUpdate>(str);
                        var person = Person.ConvertFromJson(json);
                        await App.Database.SaveItem(person);

                        App.ToastNotificator.Show("Данные успешно отправлены");

                        var page = new EditPersonPage(person.Id);
                        await App.GlobalPage.ReplaceCurrentPage(page);
                    }
                }
            } catch {
                Device.BeginInvokeOnMainThread(() =>
                    App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка добавления", "ОК"));
            }
        }

    }

}