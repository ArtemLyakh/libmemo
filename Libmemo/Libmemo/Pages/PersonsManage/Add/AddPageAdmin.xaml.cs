using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddPageAdmin : ContentPage {
        public AddPageAdmin() {
            InitializeComponent();
            BindingContext = new AddPageAdminViewModel();
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            ((AddPageAdminViewModel)BindingContext).SetGPSTracking(false);
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            ((AddPageAdminViewModel)BindingContext).SetGPSTracking(true);
        }
    }
}
