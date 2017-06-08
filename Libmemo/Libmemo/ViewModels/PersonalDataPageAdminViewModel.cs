using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public class PersonalDataPageAdminViewModel : BasePersonalDataViewModel {

        public PersonalDataPageAdminViewModel() : base() { }

        public PersonalDataPageAdminViewModel(int id) : base() {
            LoadData(id);
        }

        protected override PersonData PersonData { get; set; } = null;

        private User _owner;
        public User User {
            get => _owner;
            set {
                if (_owner != value) {
                    _owner = value;
                    this.OnPropertyChanged(nameof(User));
                    this.OnPropertyChanged(nameof(OwnerText));
                }
            }
        }
        public string OwnerText { get => this.User == null ? "Не выбрано" : $"{this.User.Id}: {this.User.FIO}"; }

        public ICommand SelectOwnerCommand {
            get => new Command(async () => {
                var searchPage = new SearchPage(await App.Database.GetItems<User>());
                searchPage.ItemSelected += async (sender, id) => {
                    this.User = await App.Database.GetById<User>(id);
                    LoadData(this.User.Owner);
                };                 

                await App.CurrentNavPage.Navigation.PushAsync(searchPage);
            });
        }






        protected async void LoadData(int id) {
            var uri = new UriBuilder(Settings.PersonalDataUrlAdmin) { Query = $"id={id}" }.Uri;

            var loader = new PersonDataLoader(uri);

            try {
                PersonData = await loader.GetPersonData();
            } catch (UnauthorizedAccessException) {
                AuthHelper.Relogin();
                return;
            }

            if (PersonData == null) {
                App.ToastNotificator.Show("Ошибка загрузки текущих данных");
            } else {
                App.ToastNotificator.Show("Данные с сервера получены");
                this.ResetCommand.Execute(null);
            }

        }





        protected override async Task AddParams(PersonDataLoader uploader) {
            await base.AddParams(uploader);
            uploader.Params.Add("id", this.User.Owner.ToString());
        }



        protected override async void Send() {
            if (this.User == null) {
                Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", "Не указан пользователь", "ОК"));
                return;
            }

            App.ToastNotificator.Show("Отправка на сервер");

            PersonDataLoader uploader = new PersonDataLoader(Settings.PersonalDataUrlAdmin);
            await AddParams(uploader);

            try {
                var success = await uploader.Upload();

                if (success) {
                    App.ToastNotificator.Show("Данные успешно отправлены");
                    LoadData(this.User.Owner);
                } else {
                    Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка отправки данных", "ОК"));
                }
            } catch (UnauthorizedAccessException) {
                AuthHelper.Relogin();
            }
        }
    }
}
