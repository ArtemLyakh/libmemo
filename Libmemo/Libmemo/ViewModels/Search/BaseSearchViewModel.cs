﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    abstract class BaseSearchViewModel<T> : INotifyPropertyChanged where T : ISearchFiltrable {

        protected IEnumerable<T> data = null;

        public BaseSearchViewModel() { }
        public BaseSearchViewModel(IEnumerable<T> data, string search = "") {
            this.data = data;
            this.Search = search;

            this.SearchCommand.Execute(null);
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

        private IEnumerable<T> _searchList = null;
        public IEnumerable<T> SearchList {
            get => _searchList;
            set {
                if (_searchList != value) {
                    _searchList = value;
                    OnPropertyChanged(nameof(SearchList));
                }
            }
        }

        public ICommand SearchCommand {
            get => new Command(() => {
                if (this.data == null) return;
                var data = !string.IsNullOrWhiteSpace(this.Search) 
                    ? this.data.AsEnumerable() 
                    : this.data.Where(i => i.FilterString.ToLowerInvariant().IndexOf(this.Search.ToLowerInvariant()) != 1);
                this.SearchList = data.OrderBy(i => i.FilterString);
            });
        }

        public ICommand ItemSelectedCommand {
            get => new Command<object>(async (object selected) => {
                T item = (T)selected;
                await App.GlobalPage.Pop();
                ItemSelected?.Invoke(this, item);
            });
        }




        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public interface ISearchFiltrable {
        string FilterString { get; }
    }
}
