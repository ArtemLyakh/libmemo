using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    class AddPageAdminViewModel : AddPageViewModel {

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
                searchPage.ItemSelected += async (sender, id) => {
                    this.Owner = await App.Database.GetById<User>(id);
                };

                await App.CurrentNavPage.Navigation.PushAsync(searchPage);
            });
        }




        protected override string SendUri => Settings.AddPersoAdminUrl;

        protected override IEnumerable<string> ValidateSend() => base.ValidateSend().Union(_ValidateSend());
        private IEnumerable<string> _ValidateSend() {
            if (this.Owner == null) yield return "Не указан владелец";
        }

        protected async override Task AddParams(PersonDataLoader uploader) {
            await base.AddParams(uploader);
            uploader.Params.Add("owner", this.Owner.Id.ToString());
        }
    }
}
