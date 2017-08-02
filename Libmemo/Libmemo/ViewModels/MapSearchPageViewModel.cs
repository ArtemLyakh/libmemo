using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;

namespace Libmemo {
    public class MapSearchPageViewModel : BaseListViewModel<ListElement.ImageElement> {

        public MapSearchPageViewModel(string search = null) : base() {
            this.Search = search;
        }

        public ICommand BackCommand => new Command(async () => await App.GlobalPage.Pop());

        public ICommand LoadCommand => new Command(async () => await Load());
        
        private async Task Load() {
            this.Data = (await App.Database.GetList(PersonType.Dead))
                .Select(i => new ListElement.ImageElement {
                    FIO = i.FIO,
                    Image = i.SmallImageUrl == null ? ImageSource.FromFile("no_img.png") : ImageSource.FromUri(i.SmallImageUrl),
                    Person = i
                }).ToList();

            this.SearchCommand.Execute(null);
        }


    }
}
