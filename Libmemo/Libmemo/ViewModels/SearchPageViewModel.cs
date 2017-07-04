using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;

namespace Libmemo {
    public class SearchPageViewModel : BaseListViewModel<SearchPageViewModel.Person> {
        public class Person : ISearchFiltrable {
            public string FilterString => Fio;

            public ImageSource Image { get; set; }
            public int Id { get; set; }
            public string Fio { get; set; }
        }

        public SearchPageViewModel(string search = null) : base() {
            this.Search = search;
            LoadCommand.Execute(null);
        }

        public ICommand LoadCommand => new Command(async () => await Load());
        
        private async Task Load() {
            this.Data = (await App.Database.GetList(PersonType.Dead))
                .Select(i => new Person {
                    Id = i.Id,
                    Fio = i.FIO,
                    Image = i.IconUrl == null ? null : ImageSource.FromUri(i.IconUrl)
                });

            this.SearchCommand.Execute(null);
        }

    }
}
