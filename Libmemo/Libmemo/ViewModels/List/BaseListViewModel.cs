using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public abstract class BaseListViewModel<T> : BaseViewModel where T : ListElement.ISearchFilterable {

        private List<T> _data = null;
        protected List<T> Data {
            get => _data;
            set {
                _data = value;
                this.SearchList = value;
            }
        }

        public BaseListViewModel() {
            SearchChanged += (sender, e) => SearchCommand.Execute(null);
        }


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

        private List<T> _searchList = null;
        public List<T> SearchList {
            get => _searchList;
            private set {
                if (_searchList != value) {
                    _searchList = value;
                    OnPropertyChanged(nameof(SearchList));
                }
            }
        }

        public ICommand SearchCommand => new Command(() => {
            if (this.Data == null) return;

            var data = string.IsNullOrWhiteSpace(this.Search) 
                ? this.Data.AsEnumerable() 
                : this.Data.Where(i => i.Filter.ToLowerInvariant().IndexOf(this.Search.ToLowerInvariant()) != -1);

            this.SearchList = data.OrderBy(i => i.Filter).ToList();
        });


        public ICommand ItemSelectedCommand => 
            new Command<object>(selected => ItemSelected?.Invoke(this, (T)selected));

    }


    namespace ListElement {

        public interface ISearchFilterable {
            string Filter { get; }
        }

        public class ImageElement : ISearchFilterable {
            public string Filter => FIO;

            public string FIO { get; set; }
            public ImageSource Image { get; set; }

            public Person Person { get; set; }       
        }

        public class TextElement : ISearchFilterable {
            public string Filter => Email;
            public string ShowString => $"{Id}: {Email}";

            public int Id { get; set; }
            public string Email { get; set; }
            public string Fio { get; set; }
        }
    }
}
