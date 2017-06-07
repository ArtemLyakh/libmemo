using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    class AddPageAdminViewModel : BaseAddViewModel {

        public AddPageAdminViewModel() : base() { }

        private User _owner;
        public User Owner {
            get => _owner; 
            set {
                if (_owner != value) {
                    _owner = value;
                    this.OnPropertyChanged(nameof(Owner));
                    this.OnPropertyChanged(nameof(OwnerText));
                }
            }
        }
        public string OwnerText { get => this.Owner == null ? "Не выбрано" : $"{this.Owner.Id}: {this.Owner.FIO}"; }

        public ICommand SelectOwnerCommand {
            get => new Command(async () => {
                var searchPage = new SearchPage(await App.Database.GetItems<User>());
                searchPage.ItemSelected += async (sender, id) => 
                    this.Owner = await App.Database.GetById<User>(id);

                await App.CurrentNavPage.Navigation.PushAsync(searchPage);
            });
        }



        protected override IEnumerable<string> Validate() => base.Validate().Concat(_Validate());
        private IEnumerable<string> _Validate() {
            if (this.Owner == null) yield return "Не указан владелец";
        }

        protected async override Task AddParams(PersonDataLoader uploader) {
            await base.AddParams(uploader);
            uploader.Params.Add("owner", this.Owner.Id.ToString());
        }



        protected override async void Send() {
            var errors = Validate();
            if (errors.Count() > 0) {
                Device.BeginInvokeOnMainThread(() => App.Current.MainPage.DisplayAlert("Ошибка", string.Join("\n", errors), "ОК"));
                return;
            }

            App.ToastNotificator.Show("Отправка на сервер");

            var uploader = new PersonDataLoader(Settings.AddPersoAdminUrl);
            await AddParams(uploader);

            try {
                var success = await uploader.Upload();

                if (success) {
                    App.ToastNotificator.Show("Данные успешно отправлены");
                    ResetCommand.Execute(null);
                    App.Database.Load();
                } else {
                    Device.BeginInvokeOnMainThread(() => App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка отправки данных", "ОК"));
                }
            } catch (UnauthorizedAccessException) {
                AuthHelper.Relogin();
            }
        }
    }
}
