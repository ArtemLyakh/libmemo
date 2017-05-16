using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage {
        public MapPage() {

            BindingContext = new MainPageViewModel(this);
            InitializeComponent();

        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            ((MainPageViewModel)BindingContext).SetGPSTracking(false);
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            ((MainPageViewModel)BindingContext).SetGPSTracking(true);
        }
    }

}
