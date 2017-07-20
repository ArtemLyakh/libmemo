using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;

namespace Libmemo {
    public class MapSearchPageViewModel : BaseListViewModel<MapSearchPageViewModel.Person> {
        public class Person : ISearchFiltrable {
            public string FilterString => Fio;

            public object Image { get; set; }
            public int Id { get; set; }
            public string Fio { get; set; }
        }

        public MapSearchPageViewModel(string search = null) : base() {
            this.Search = search;
            this.SearchChanged += (sender, e) => SearchCommand.Execute(null);
        }

        public ICommand BackCommand => new Command(async () => await App.GlobalPage.Pop());

        public ICommand LoadCommand => new Command(async () => await Load());
        
        private async Task Load() {
            this.Data = (await App.Database.GetList(PersonType.Dead))
                .Select(i => new Person {
                    Id = i.Id,
                    Fio = i.FIO,
                    Image = i.SmallImageUrl == null ? "no_img.png" : ImageSource.FromUri(i.SmallImageUrl)
                });

            this.SearchCommand.Execute(null);
        }


    }
}
