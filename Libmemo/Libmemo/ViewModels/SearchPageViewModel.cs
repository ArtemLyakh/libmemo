using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;

namespace Libmemo {
    public class SearchPageViewModel : INotifyPropertyChanged {

        private IEnumerable<IDatabaseSavable> data;

        public SearchPageViewModel(IEnumerable<IDatabaseSavable> data, string search = null) {
            this.data = data;
            this.Search = search;

            GetItems();
        }

        public event EventHandler<string> SearchTextChanged;
        public event EventHandler<int> ItemSelected;



        private void GetItems() {
            IEnumerable<T> WhereIf<T>(IEnumerable<T> data, bool filter, Func<T, bool> func) =>
                filter ? data.Where(func) : data;

            this.SearchList = WhereIf(
                data,
                !string.IsNullOrWhiteSpace(this.Search),
                i => i.FIO.ToLowerInvariant().IndexOf(this.Search.ToLowerInvariant()) != -1
            )
            .OrderBy(i => i.FIO)
            .Select(i => Tuple.Create(i.Id, i.FIO));
        }

        private string search;
        public string Search {
            get { return search; }
            set {
                if (search != value) {
                    search = value;
                    OnPropertyChanged(nameof(Search));
                    SearchTextChanged?.Invoke(this, value);
                }
            }
        }

        private IEnumerable<Tuple<int, string>> searchList;
        public IEnumerable<Tuple<int, string>> SearchList {
            get { return searchList; }
            set {
                if (searchList != value) {
                    searchList = value;
                    OnPropertyChanged(nameof(SearchList));
                }
            }
        }



        public ICommand SearchCommand {
            get => new Command(() => GetItems());
        }

        public ICommand ItemSelectedCommand {
            get => new Command<Tuple<int, string>>(async (Tuple<int, string> item) => {
                await App.GlobalPage.Pop();

                SearchTextChanged?.Invoke(this, item.Item2);
                ItemSelected?.Invoke(this, item.Item1);
            });
        }



        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
