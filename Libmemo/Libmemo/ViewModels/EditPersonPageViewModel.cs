using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo {
    public class EditPersonPageViewModel : INotifyPropertyChanged {

        private int id;
        public EditPersonPageViewModel(int id) {
            this.id = id;
        }

        public async void Init() {

        }




        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
