using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PersonCollectionAdminPage : ContentPage {
        public PersonCollectionAdminPage() {
            InitializeComponent();
            BindingContext = new PersonCollectionAdminPageViewModel();
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            ((PersonCollectionAdminPageViewModel)BindingContext).LoadCommand.Execute(null);
        }
    }
}
