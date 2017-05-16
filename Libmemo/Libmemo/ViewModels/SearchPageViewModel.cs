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

        public SearchPageViewModel(string search, EventHandler<Person> OnItemSelected = null, EventHandler<string> OnSearchChanged = null) {
            this.search = search;
            this.OnItemSelected = OnItemSelected;
            this.OnSearchChanged = OnSearchChanged;
            GetItems(search);
        }

        #region Utils

        private void GetItems(string search) {
            Task.Factory.StartNew(async () => {
                SearchList = await App.Database.GetItems(search);
            });
        }

        #endregion

        #region Callbacks

        public EventHandler<string> OnSearchChanged = null;
        public EventHandler<Person> OnItemSelected = null;

        #endregion

        #region Properties

        private string search;
        public string Search {
            get { return search; }
            set {
                if (search != value) {
                    search = value;
                    OnPropertyChanged(nameof(Search));
                    OnSearchChanged?.Invoke(this, value);
                }
            }
        }

        private IEnumerable<Person> searchList;
        public IEnumerable<Person> SearchList {
            get { return searchList; }
            set {
                if (searchList != value) {
                    searchList = value;
                    OnPropertyChanged(nameof(SearchList));
                }
            }
        }

        #endregion

        #region Commands

        public ICommand SearhCommand {
            get {
                return new Command(() => {
                    if (String.IsNullOrWhiteSpace(Search)) return;
                    GetItems(this.Search);
                });
            }
        }

        public ICommand ItemSelectedCommand {
            get {
                return new Command(async (object item) => {
                    OnItemSelected?.Invoke(this, (Person)item);
                    await ((Application.Current.MainPage as MainPage)?.Detail as NavigationPage)?.Navigation.PopAsync();
                });
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
