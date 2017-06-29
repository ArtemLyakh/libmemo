using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage {
        public MapPage() {

            BindingContext = new MapPageViewModel();
            InitializeComponent();

        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            ((MapPageViewModel)BindingContext).SetGPSTracking(false);
            ((MapPageViewModel)BindingContext).StopListen();

        }

        protected override void OnAppearing() {
            base.OnAppearing();
            ((MapPageViewModel)BindingContext).SetGPSTracking(true);
            ((MapPageViewModel)BindingContext).StartListen();

            ((MapPageViewModel)BindingContext).SetupPins.Execute(null);
        }

    }

}
