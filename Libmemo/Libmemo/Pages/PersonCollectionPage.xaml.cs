using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PersonCollectionPage : ContentPage {
        public PersonCollectionPage() {
            InitializeComponent();
            BindingContext = new PersonCollectionPageViewModel();
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            ((PersonCollectionPageViewModel)BindingContext).LoadCommand.Execute(null);
        }

    }
}
