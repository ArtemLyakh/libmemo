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
            try {
                InitializeComponent();
            } catch (Exception e) {
                var q = 1;
            }
            BindingContext = new EditPersonPageViewModel(id);
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            ((EditPersonPageViewModel)BindingContext).SetGPSTracking(true);
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            ((EditPersonPageViewModel)BindingContext).SetGPSTracking(false);
        }
    }
}
