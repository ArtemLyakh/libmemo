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

        protected PersonData PersonData { get; set; } = null;

        private User _user;
        public User User {
            get => _user;
            set {
                if (_user != value) {
                    _user = value;
                    this.OnPropertyChanged(nameof(User));
                    this.OnPropertyChanged(nameof(OwnerText));
                }
            }
        }
        public string OwnerText { get => this.User == null ? "Не выбрано" : $"{this.User.Id}: {this.User.FIO}"; }

        public ICommand SelectOwnerCommand {
            get => new Command(async () => {
                //var searchPage = new SearchPage(await App.Database.GetItems<User>());
                SearchPage searchPage = null;
                searchPage.ItemSelected += async (sender, id) => {
                    //this.User = await App.Database.GetById<User>(id);
                    //LoadData(this.User.Owner);
                };

                await App.GlobalPage.Push(searchPage);
            });
        }



        private int _dbId = default(int);
        private async void LoadSuccess() {
            App.Database.LoadSuccess -= LoadSuccess;
            App.Database.LoadFail -= LoadFail;
            //this.User = (await App.Database.GetItems<User>()).Where(i => i.Owner == _dbId).FirstOrDefault();         
        }
        private async void LoadFail() {
            App.Database.LoadSuccess -= LoadSuccess;
            App.Database.LoadFail -= LoadFail;
            Device.BeginInvokeOnMainThread(() => App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка синхронизации", "ОК"));
            await App.GlobalPage.PopToRootPage();
        }

        protected async void LoadData(int id) {
            //var user = (await App.Database.GetItems<User>()).Where(i => i.Owner == id).FirstOrDefault();
            User user = null;
            if (user != null) {
                this.User = user;
            } else {
                _dbId = id;
                App.Database.LoadSuccess += LoadSuccess;
                App.Database.LoadFail += LoadFail;
                App.Database.Load();
            }



            var uri = new UriBuilder(Settings.PERSONAL_DATA_URL_ADMIN) { Query = $"id={id}" }.Uri;

            var loader = new PersonDataLoader(uri);

            try {
                PersonData = await loader.GetPersonData();
            } catch (UnauthorizedAccessException) {
                await AuthHelper.ReloginAsync();
                return;
            }

            if (PersonData == null) {
                App.ToastNotificator.Show("Ошибка загрузки текущих данных");
            } else {
                App.ToastNotificator.Show("Данные с сервера получены");
                this.ResetCommand.Execute(null);
            }

        }





        //protected override async Task AddParams(PersonDataLoader uploader) {
        //    await base.AddParams(uploader);
        //    uploader.Params.Add("id", this.User.Owner.ToString());
        //}



        protected override async Task Send() {
            if (this.User == null) {
                Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", "Не указан пользователь", "ОК"));
                return;
            }

            App.ToastNotificator.Show("Отправка на сервер");

            PersonDataLoader uploader = new PersonDataLoader(Settings.PERSONAL_DATA_URL_ADMIN);
            //await AddParams(uploader);

            try {
                var success = await uploader.Upload();

                if (success) {
                    App.ToastNotificator.Show("Данные успешно отправлены");
                    LoadData(this.User.Owner);
                } else {
                    Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка отправки данных", "ОК"));
                }
            } catch (UnauthorizedAccessException) {
                await AuthHelper.ReloginAsync();
            }
        }

        protected override Task Reset() {
            throw new NotImplementedException();
        }
    }
}
