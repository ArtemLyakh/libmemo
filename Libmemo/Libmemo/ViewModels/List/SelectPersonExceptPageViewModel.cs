using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public class SelectPersonExceptPageViewModel : BaseListViewModel<SelectPersonExceptPageViewModel.ListItem> {
        public class ListItem : ISearchFiltrable {
            public string FilterString => Fio;

            public ImageSource Image { get; set; }
            public int Id { get; set; }
            public string Fio { get; set; }

            public Person Person { get; set; }
        }

        private IEnumerable<int> Except { get; set; }

        public SelectPersonExceptPageViewModel(IEnumerable<int> except) : base() {
            Except = except;
        }

        public ICommand LoadCommand => new Command(async () => await Load());

        private async Task Load() {
            var dict = await App.Database.GetDictionary();
            foreach (var id in Except) 
                if (dict.ContainsKey(id)) dict.Remove(id);

            this.Data = dict.Select(i => new ListItem {
                Id = i.Value.Id,
                Fio = i.Value.FIO,
                Image = i.Value.IconUrl == null ? null : ImageSource.FromUri(i.Value.IconUrl),
                Person = i.Value
            });
        }
    }
}
