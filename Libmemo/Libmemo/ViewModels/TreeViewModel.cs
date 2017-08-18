using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public abstract class TreeViewModel : BaseViewModel {

        protected Tree Tree { get; set; }

        public TreeViewModel(int? id, AbsoluteLayout absolute, ScrollView scroll) : base() {
            if (id.HasValue) {
                Tree = new Tree(id.Value, absolute, scroll);
            } else {
                AuthHelper.ReloginAsync().RunSynchronously();
            }
        }

        public ICommand ZoomInCommand => new Command(async () => await Tree?.ZoomIn());
        public ICommand ZoomOutCommand => new Command(async () => await Tree?.ZoomOut());


        protected abstract void Save();
        public ICommand SaveCommand => new Command(Save);

        protected abstract void Reset();
        public ICommand ResetCommand => new Command(Reset);

    }
}
