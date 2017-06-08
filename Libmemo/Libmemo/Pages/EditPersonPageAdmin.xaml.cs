using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditPersonPageAdmin : ContentPage {
        public EditPersonPageAdmin(int id) {
            InitializeComponent();
            BindingContext = new EditPersonPageAdminViewModel(id);
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            ((EditPersonPageAdminViewModel)BindingContext).SetGPSTracking(true);
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            ((EditPersonPageAdminViewModel)BindingContext).SetGPSTracking(false);
        }

    }
}
