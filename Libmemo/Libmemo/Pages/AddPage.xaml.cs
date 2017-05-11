using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Libmemo {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddPage : ContentPage {
        public AddPage(Position userPosition, EventHandler OnItemAdded = null) {
            InitializeComponent();
            BindingContext = new AddPageViewModel(userPosition, OnItemAdded);
        }
    }

}
