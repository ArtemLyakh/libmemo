using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditPersonPage : ContentPage {
        private ViewModel Model {
            get => (ViewModel)BindingContext;
            set => BindingContext = value;
        }

        public EditPersonPage(int id) {
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

            public override void OnAppearing() {
                base.OnAppearing();
                SetGPSTracking(true);
            }

            public override void OnDisappearing() {
                base.OnDisappearing();
                SetGPSTracking(false);
            }

            protected override async void Send() {
                if (cancelTokenSource != null) return;


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
                content.Add(new StringContent(IsDeadPerson ? "dead" : "alive"), "type");
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



                HttpResponseMessage response = null;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Post, new Uri(Settings.EDIT_PERSON_URL), content, 60, cancelTokenSource.Token);
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
                    response = await WebClient.Instance.SendAsync(HttpMethod.Post, new Uri(Settings.DELETE_PERSON_URL), content, 15, cancelTokenSource.Token);
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
