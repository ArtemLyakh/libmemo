using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public abstract class BaseViewModel : INotifyPropertyChanged {

        public BaseViewModel() { }

        public ICommand BackCommand => new Command(async () => await App.GlobalPage.Pop());

        public virtual void OnAppearing() { }
        public virtual void OnDisappearing() { }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
