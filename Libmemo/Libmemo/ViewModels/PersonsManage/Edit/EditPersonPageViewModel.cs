using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo {
    public class EditPersonPageViewModel : BaseEditPersonViewModel {

        public EditPersonPageViewModel(int id) : base(id) { }

        protected override async void Send() {
            var errors = new List<string>();
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
                    content.Add(new StringContent(this.Id.ToString()), "id");
                    content.Add(new StringContent(this.IsDeadPerson ? "dead" : "alive"), "type");
                    content.Add(new StringContent(this.FirstName), "first_name");
                    content.Add(new StringContent(this.SecondName), "second_name");
                    content.Add(new StringContent(this.LastName), "last_name");
                    if (this.DateBirth.HasValue)
                        content.Add(new StringContent(this.DateBirth.Value.ToString("yyyy-MM-dd")), "date_birth");
                    if (this.PhotoSource != null && this.PhotoSource is FileImageSource) {
                        var result = await DependencyService.Get<IImageFileToByteArrayConverter>().Get(this.PhotoSource);
                        content.Add(new ByteArrayContent(result), "photo", "photo.jpg");
                    }

                    if (this.IsDeadPerson) {
                        content.Add(new StringContent(this.PersonPosition.Latitude.ToString(CultureInfo.InvariantCulture)), "latitude");
                        content.Add(new StringContent(this.PersonPosition.Longitude.ToString(CultureInfo.InvariantCulture)), "longitude");
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

                    using (var message = await client.PostAsync(Settings.EDIT_PERSON_URL, content)) {
                        if (message.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                            await AuthHelper.ReloginAsync();
                            return;
                        }

                        message.EnsureSuccessStatusCode();

                        App.ToastNotificator.Show("Данные успешно отправлены");
                        App.Database.Load();
                        await App.GlobalPage.Pop();
                        //bool success = await App.Database.Load();
                        //if (success) ResetCommand.Execute(null);
                    }
                }
            } catch {
                Device.BeginInvokeOnMainThread(() =>
                    App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка отправки данных", "ОК"));
            }


        }  

    }
}
