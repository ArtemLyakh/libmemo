﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditPersonPageAdmin : ContentPage {
        private ViewModel Model {
            get => (ViewModel)BindingContext;
            set => BindingContext = value;
        }

        public EditPersonPageAdmin(int id) {
            InitializeComponent();
            BindingContext = new ViewModel(id);
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            Model.OnAppearing();
        }
        protected override void OnDisappearing() {
            base.OnDisappearing();
            Model.OnDisappearing();
        }



        public class ViewModel : BaseEditPersonViewModel {

            public ViewModel(int id) : base(id) { }

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

            protected async override void Reset() {
                if (cancelTokenSource != null) return;

                var person = await App.Database.GetById(Id);

                var builder = new UriBuilder(Settings.USER_DATA_URL);
                builder.Query = $"id={person.Owner}";
                var uri = builder.Uri;


                HttpResponseMessage response = null;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Get, uri, null, 10, cancelTokenSource.Token);
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
                var json = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.User>(str);
                Owner = new User { Id = json.id, Fio = json.fio, Email = json.fio };

                base.Reset();
            }

            protected override async void Send() {
                if (cancelTokenSource != null) return;


                if (this.Owner == null) {
                    App.ToastNotificator.Show("Не указан владелец");
                    return;
                }
                if (this.IsDeadPerson && this.PersonPosition == default(Position)) {
                    App.ToastNotificator.Show("Не указано местоположение");
                    return;
                }
                if (string.IsNullOrWhiteSpace(this.FirstName)) {
                    App.ToastNotificator.Show("Поле \"Имя\" не заполнено");
                    return;
                }


                StartLoading("Сохранение");



                var content = new MultipartFormDataContent(String.Format("----------{0:N}", Guid.NewGuid()));
                content.Add(new StringContent(Id.ToString()), "id");
                content.Add(new StringContent(this.Owner.Id.ToString()), "owner");
                content.Add(new StringContent(IsDeadPerson ? "dead" : "alive"), "type");
                content.Add(new StringContent(this.FirstName), "first_name");
                if (!string.IsNullOrWhiteSpace(this.SecondName))
                    content.Add(new StringContent(this.SecondName), "second_name");
                if (!string.IsNullOrWhiteSpace(this.LastName))
                    content.Add(new StringContent(this.LastName), "last_name");
                if (this.DateBirth.HasValue)
                    content.Add(new StringContent(this.DateBirth.Value.ToString("yyyy-MM-dd")), "date_birth");
                if (this.PhotoSource != null && this.PhotoSource is FileImageSource) {
					var result = await DependencyService.Get<IFileStreamPicker>().GetResizedJpegAsync((PhotoSource as FileImageSource).File, 1000, 1000);
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



                HttpResponseMessage response = null;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Post, new Uri(Settings.EDIT_PERSON_URL_ADMIN), content, 60, cancelTokenSource.Token);
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
                response.EnsureSuccessStatusCode();
                var json = Newtonsoft.Json.JsonConvert.DeserializeObject<PersonJson.Update>(str);
                var person = Person.ConvertFromJson(json);
                await App.Database.AddPerson(person);

                ResetCommand.Execute(null);

            }


            protected override async void Delete() {
                if (cancelTokenSource != null) return;


                if (!await App.Current.MainPage.DisplayAlert("Удаление", "Вы уверены?", "Да", "Нет")) return;


                StartLoading("Удаление");


                var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                    { "id", this.Id.ToString() }
                });



                HttpResponseMessage response = null;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Post, new Uri(Settings.DELETE_PERSON_URL_ADMIN), content, 15, cancelTokenSource.Token);
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
                response.EnsureSuccessStatusCode();
                var json = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.PersonDelete>(str);

                await App.Database.DeletePerson(json.id);

                await App.GlobalPage.Pop();

            }

        }


    }
}
