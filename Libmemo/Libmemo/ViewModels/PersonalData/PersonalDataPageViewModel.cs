using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public class PersonalDataPageViewModel : BasePersonalDataViewModel {

        public PersonalDataPageViewModel() : base() {
            this.ResetCommand.Execute(null);
        }


        class JsonData {
            public string first_name { get; set; }
            public string second_name { get; set; }
            public string last_name { get; set; }
            public string date_birth { get; set; }
            public string photo_url { get; set; }
        }
        protected async Task LoadData() {
            try {
                using (var handler = new HttpClientHandler { CookieContainer = AuthHelper.CookieContainer })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) })
                using (var responce = await client.GetAsync(Settings.PERSONAL_DATA_URL)) {

                    if (responce.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        await AuthHelper.ReloginAsync();
                        return;
                    }

                    responce.EnsureSuccessStatusCode();
                    var str = await responce.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<JsonData>(str);

                    this.FirstName = data.first_name;
                    this.SecondName = data.second_name;
                    this.LastName = data.last_name;
                    this.DateBirth = DateTime.TryParse(data.date_birth, out DateTime dateBirth) ? (DateTime?)dateBirth : null;
                    this.PhotoSource = Uri.TryCreate(data.photo_url, UriKind.Absolute, out Uri photoUri)
                        ? new UriImageSource { CachingEnabled = true, Uri = photoUri }
                        : null;

                    App.ToastNotificator.Show("Данные получены");
                }
            } catch {
                App.ToastNotificator.Show("Ошибка загрузки данных");
            }

        }

        protected override async Task Reset() => await LoadData();

        protected override async Task Send() {
            try {
                using (var handler = new HttpClientHandler { CookieContainer = AuthHelper.CookieContainer })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) })
                using (var content = new MultipartFormDataContent(String.Format("----------{0:N}", Guid.NewGuid()))) {
                    content.Add(new StringContent(this.FirstName), "first_name");
                    content.Add(new StringContent(this.SecondName), "second_name");
                    content.Add(new StringContent(this.LastName), "last_name");
                    if (this.DateBirth.HasValue) {
                        content.Add(new StringContent(this.DateBirth.Value.ToString("yyyy-MM-dd")), "date_birth");
                    }
                    if (this.PhotoSource != null && this.PhotoSource is FileImageSource) {
                        var result = await DependencyService.Get<IImageFileToByteArrayConverter>().Get(this.PhotoSource);
                        content.Add(new ByteArrayContent(result), "photo", "photo.jpg");
                    }

                    using (var response = await client.PostAsync(Settings.PERSONAL_DATA_URL, content)) {
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                            await AuthHelper.ReloginAsync();
                            return;
                        }

                        response.EnsureSuccessStatusCode();

                        var str = await response.Content.ReadAsStringAsync();
                        var json = JsonConvert.DeserializeObject<PersonJson.Update>(str);
                        var person = Person.ConvertFromJson(json);
                        await App.Database.SaveItem(person);

                        App.ToastNotificator.Show("Данные успешно отправлены");
                        this.ResetCommand.Execute(null);
                    }
                }
            } catch {
                Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка отправки данных", "ОК"));
            }
        }

    }
}
