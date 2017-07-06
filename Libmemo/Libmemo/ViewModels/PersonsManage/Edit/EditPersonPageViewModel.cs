﻿using Plugin.Media;
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
                    content.Add(new StringContent(Id.ToString()), "id");
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

                    using (var response = await client.PostAsync(Settings.EDIT_PERSON_URL, content)) {
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                            await AuthHelper.ReloginAsync();
                            return;
                        }

                        var str = await response.Content.ReadAsStringAsync();
                        response.EnsureSuccessStatusCode();
                        var json = Newtonsoft.Json.JsonConvert.DeserializeObject<PersonJson.Update>(str);
                        var person = Person.ConvertFromJson(json);
                        await App.Database.SaveItem(person);

                        App.ToastNotificator.Show("Данные успешно отправлены");

                        ResetCommand.Execute(null);
                    }
                }
            } catch {
                Device.BeginInvokeOnMainThread(() =>
                    App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка редактирования", "ОК"));
            }


        }  

    }
}
