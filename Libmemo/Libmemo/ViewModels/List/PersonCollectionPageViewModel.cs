using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public class PersonCollectionPageViewModel : BaseListViewModel<ListElement.ImageElement> {

        public PersonCollectionPageViewModel() : base() {
            this.ItemSelected += async (sender, e) => await App.GlobalPage.Push(new EditPersonPage(e.Person.Id));
        }

        public ICommand BackCommand => new Command(async () => await App.GlobalPage.Pop());

        public ICommand AddCommand => new Command(async () => await App.GlobalPage.Push(new AddPage()));

        public ICommand LoadCommand => new Command(async () => await Load());

        private async Task Load() {
            if (!AuthHelper.IsLogged) {
                await AuthHelper.ReloginAsync();
                return;
            }

            this.Data = (await App.Database.GetList(new PersonType[] { PersonType.Alive, PersonType.Dead }))
                .Where(i => i.Owner == AuthHelper.CurrentUserId.Value)
                .Select(i => new ListElement.ImageElement {
                    FIO = i.FIO,
                    Image = i.SmallImageUrl == null ? ImageSource.FromFile("no_img.png") : ImageSource.FromUri(i.SmallImageUrl),
                    Person = i
                }).ToList();

        }

    }
}
