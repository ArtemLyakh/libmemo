using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    class EditPersonPageAdminViewModel : BaseEditPersonViewModel {

        public EditPersonPageAdminViewModel(int id) : base(id) { }

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
                searchPage.ItemSelected += async (sender, id) => {
                    this.Owner = await App.Database.GetById<User>(id);
                };

                await App.GlobalPage.Push(searchPage);
            });
        }


        protected override async void InitFields(Person person) {
            base.InitFields(person);
            var owner = (await App.Database.GetItems<User>()).FirstOrDefault(i => i.Owner == person.Owner);
            if (owner != null) {
                Owner = owner;
            }
        }

        protected override async Task AddParams(PersonDataLoader uploader) {
            await base.AddParams(uploader);
            uploader.Params.Add("owner", this.Owner.Id.ToString());
        }

        protected override IEnumerable<string> Validate() {
            return base.Validate().Concat(_Validate());
        }
        private IEnumerable<string> _Validate() {
            if (Owner == null) yield return "Владелец не задан";
        }

        protected override async void Send() {
            var errors = Validate();
            if (errors.Count() > 0) {
                Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", string.Join("\n", errors), "ОК"));
                return;
            }

            App.ToastNotificator.Show("Отправка на сервер");

            PersonDataLoader uploader = new PersonDataLoader(Settings.EditPersonUrlAdmin);

            await AddParams(uploader);

            try {
                var success = await uploader.Upload();

                if (success) {
                    App.ToastNotificator.Show("Данные успешно отправлены");
                    App.Database.Load();
                    await App.GlobalPage.PopToRootPage();
                } else {
                    Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка отправки данных", "ОК"));
                }
            } catch (UnauthorizedAccessException) {
                await AuthHelper.ReloginAsync();
            }
        }

    }


}

