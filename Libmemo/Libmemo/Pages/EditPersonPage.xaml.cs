using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditPersonPage : ContentPage {
        public EditPersonPage(int id) {
            InitializeComponent();
            BindingContext = new EditPersonPageViewModel(id);
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            (BindingContext as EditPersonPageViewModel)?.Init();
        }

    }
}
