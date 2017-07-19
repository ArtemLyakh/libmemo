using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public class PersonCollectionPageViewModel : BaseListViewModel<PersonCollectionPageViewModel.Person> {
        public class Person : ISearchFiltrable {
            public string FilterString => Fio;

            public object Image { get; set; }
            public bool IsDead { get; set; }
            public int Id { get; set; }
            public string Fio { get; set; }
        }

        public PersonCollectionPageViewModel() : base() {
            this.SearchChanged += (sender, e) => SearchCommand.Execute(null);
            this.ItemSelected += async (object sender, Person e) => await App.GlobalPage.Push(new EditPersonPage(e.Id));
        }

        public ICommand BackCommand => new Command(async () => await App.GlobalPage.Pop());

        public ICommand AddCommand => new Command(async () => await App.GlobalPage.Push(new AddPage()));

        public ICommand LoadCommand => new Command(async () => await Load());

        private async Task Load() {
            if (!AuthHelper.CurrentUserId.HasValue) {
                await AuthHelper.ReloginAsync();
                return;
            }

            this.Data = (await App.Database.GetList(new PersonType[] { PersonType.Alive, PersonType.Dead }))
                .Where(i => i.Owner == AuthHelper.CurrentUserId.Value)
                .Select(i => new Person {
                    Id = i.Id,
                    Fio = i.FIO,
                    IsDead = i.PersonType == PersonType.Dead,
                    Image = i.IconUrl == null ? "no_img.png" : ImageSource.FromUri(i.IconUrl)                              
                });

        }

    }
}
