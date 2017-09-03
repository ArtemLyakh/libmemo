using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public abstract class BaseViewModel : INotifyPropertyChanged {

        public BaseViewModel() { }

        public ICommand BackCommand => new Command(async () => await App.GlobalPage.Pop());

        public ICommand OpenMenuCommand => new Command(() => App.SetShowMenu(true));

        public virtual void OnAppearing() { }
        public virtual void OnDisappearing() {
            cancelTokenSource?.Cancel();
        }



        private bool _isLoading = false;
        public bool IsLoading {
            get => _isLoading;
            private set {
                if (_isLoading != value) {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        private string _loadingString;
        public string LoadingString {
            get => _loadingString;
            private set {
                if (_loadingString != value) {
                    _loadingString = value;
                    OnPropertyChanged(nameof(LoadingString));
                }
            }
        }

        protected void StartLoading(string loadingString) {
            LoadingString = loadingString;
            IsLoading = true;
        }
        protected void StopLoading() {
            LoadingString = null;
            IsLoading = false;
        }


        protected CancellationTokenSource cancelTokenSource { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
