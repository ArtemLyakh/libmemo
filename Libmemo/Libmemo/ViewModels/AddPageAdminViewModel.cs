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
            get { return _owner; }
            set {
                if (_owner != value) {
                    _owner = value;
                    this.OnPropertyChanged(nameof(Owner));
                }
            }
        }
        public string OwnerText { get => this.Owner == null ? "Не выбрано" : $"{this.Owner.Id}: {this.Owner.Name}"; }

        public ICommand SelectOwnerCommand {
            get {
                return new Command(() => {
                    Device.BeginInvokeOnMainThread(() => {
                        App.ToastNotificator.Show("SelectOwnerCommand");
                    });
                });
            }
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
