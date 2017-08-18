﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PersonalDataPage : ContentPage {
        private ViewModel Model {
            get => (ViewModel)BindingContext;
            set => BindingContext = value;
        }

        public PersonalDataPage() {
            InitializeComponent();
            BindingContext = new ViewModel();
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            Model.OnAppearing();
        }
        protected override void OnDisappearing() {
            base.OnDisappearing();
            Model.OnDisappearing();
        }


        public class ViewModel : BasePersonalDataViewModel {
            public ViewModel() : base() { }

            public override void OnAppearing() {
                base.OnAppearing();
                ResetCommand.Execute(null);
            }

            protected override async void Reset() {
                if (cancelTokenSource != null) return;

                StartLoading("Получение данных");

                HttpResponseMessage response = null;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Get, new Uri(Settings.PERSONAL_DATA_URL), null, 15, cancelTokenSource.Token);
                } catch (TimeoutException) {
                    App.ToastNotificator.Show("Превышен интервал запроса");
                    return;
                } catch (OperationCanceledException) { //cancel
                    return;
                } catch {
                    App.ToastNotificator.Show("Ошибка");
                    return;
                } finally {
                    cancelTokenSource = null;
                    StopLoading();
                }
                if (response == null) return;

                try {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }
                    response.EnsureSuccessStatusCode();
                } catch (UnauthorizedAccessException) {
                    await AuthHelper.ReloginAsync();
                    return;
                } catch {
                    App.ToastNotificator.Show("Ошибка");
                    return;
                }

                var str = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Json.PersonalData>(str);

                this.FirstName = data.first_name;
                this.SecondName = data.second_name;
                this.LastName = data.last_name;
                this.DateBirth = DateTime.TryParse(data.date_birth, out DateTime dateBirth) ? (DateTime?)dateBirth : null;
                this.PhotoSource = Uri.TryCreate(data.photo_url, UriKind.Absolute, out Uri photoUri)
                    ? new UriImageSource { CachingEnabled = true, Uri = photoUri }
                    : null;

            }

            protected override async void Send() {
                if (cancelTokenSource != null) return;

                if (string.IsNullOrWhiteSpace(this.FirstName)) {
                    App.ToastNotificator.Show("Имя не может быть пустым");
                    return;
                }

                StartLoading("Сохранение");

                var content = new MultipartFormDataContent(String.Format("----------{0:N}", Guid.NewGuid()));
                content.Add(new StringContent(this.FirstName), "first_name");
                content.Add(new StringContent(this.SecondName ?? string.Empty), "second_name");
                content.Add(new StringContent(this.LastName ?? string.Empty), "last_name");
                if (this.DateBirth.HasValue) {
                    content.Add(new StringContent(this.DateBirth.Value.ToString("yyyy-MM-dd")), "date_birth");
                }
                if (this.PhotoSource != null && this.PhotoSource is FileImageSource) {
                    var result = DependencyService.Get<IFileStreamPicker>().GetStream((PhotoSource as FileImageSource).File);
                    content.Add(new StreamContent(result), "photo", "photo.jpg");
                }


                HttpResponseMessage response = null;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Post, new Uri(Settings.PERSONAL_DATA_URL), content, 60, cancelTokenSource.Token);
                } catch (TimeoutException) {
                    App.ToastNotificator.Show("Превышен интервал запроса");
                    return;
                } catch (OperationCanceledException) { //cancel
                    return;
                } catch {
                    App.ToastNotificator.Show("Ошибка");
                    return;
                } finally {
                    cancelTokenSource = null;
                    StopLoading();
                }
                if (response == null) return;

                try {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }
                    response.EnsureSuccessStatusCode();
                } catch (UnauthorizedAccessException) {
                    await AuthHelper.ReloginAsync();
                    return;
                } catch {
                    App.ToastNotificator.Show("Ошибка");
                    return;
                }


                var str = await response.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject<PersonJson.Update>(str);
                var person = Person.ConvertFromJson(json);
                await App.Database.AddPerson(person);

                this.ResetCommand.Execute(null);
            }

        }
    }
}
