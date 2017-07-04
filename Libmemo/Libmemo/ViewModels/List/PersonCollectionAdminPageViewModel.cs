using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public class PersonCollectionAdminPageViewModel : BaseListViewModel<PersonCollectionAdminPageViewModel.Person> {
        public class Person : ISearchFiltrable {
            public string FilterString => Fio;

            public ImageSource Image { get; set; }
            public bool IsDead { get; set; }
            public int Id { get; set; }
            public string Fio { get; set; }
        }

        public PersonCollectionAdminPageViewModel() : base() {
            this.ItemSelected += PersonCollectionAdminPageViewModel_ItemSelected;
        }

        private async void PersonCollectionAdminPageViewModel_ItemSelected(object sender, Person e) => 
            await App.GlobalPage.Push(new EditPersonPageAdmin(e.Id));

        public ICommand LoadCommand => new Command(async() => await Load());

        private async Task Load() {
            if (!AuthHelper.IsAdmin) {
                await AuthHelper.ReloginAsync();
                return;
            }

            this.Data = (await App.Database.GetList())
                .Select(i => new Person {
                    Id = i.Id,
                    Fio = i.FIO,
                    IsDead = i.PersonType == PersonType.Dead,
                    Image = i.IconUrl == null ? null : ImageSource.FromUri(i.IconUrl)
                });

        }
    }
}
