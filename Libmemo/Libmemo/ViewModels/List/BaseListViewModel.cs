using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public abstract class BaseListViewModel<T> : INotifyPropertyChanged where T : ISearchFiltrable {

        private IEnumerable<T> _data = null;
        protected IEnumerable<T> Data {
            get => _data;
            set {
                _data = value;
                this.SearchList = value;
            }
        }

        public BaseListViewModel() { }

        public event EventHandler<T> ItemSelected;
        public event EventHandler<string> SearchChanged;



        private string _search;
        public string Search {
            get => _search;
            set {
                if (_search != value) {
                    _search = value;
                    OnPropertyChanged(nameof(Search));
                    SearchChanged?.Invoke(this, value);
                }
            }
        }

        private IEnumerable<T> _searchList = null;
        public IEnumerable<T> SearchList {
            get => _searchList;
            private set {
                if (_searchList != value) {
                    _searchList = value;
                    OnPropertyChanged(nameof(SearchList));
                }
            }
        }

        public ICommand SearchCommand {
            get => new Command(() => {
                if (this.Data == null) return;
                var data = string.IsNullOrWhiteSpace(this.Search) 
                    ? this.Data.AsEnumerable() 
                    : this.Data.Where(i => i.FilterString.ToLowerInvariant().IndexOf(this.Search.ToLowerInvariant()) != -1);
                this.SearchList = data.OrderBy(i => i.FilterString);
            });
        }

        public ICommand ItemSelectedCommand => 
            new Command<object>(selected => ItemSelected?.Invoke(this, (T)selected));
        




        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public interface ISearchFiltrable {
        string FilterString { get; }
    }
}
