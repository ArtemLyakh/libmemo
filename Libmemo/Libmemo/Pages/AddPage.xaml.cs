using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Libmemo {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddPage : ContentPage {
        public AddPage() {
            InitializeComponent();
            BindingContext = new AddPageViewModel();
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            ((AddPageViewModel)BindingContext).SetGPSTracking(false);
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            ((AddPageViewModel)BindingContext).SetGPSTracking(true);
        }
    }

}
