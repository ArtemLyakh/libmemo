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

            var model = new PersonCollectionAdminPageViewModel();
            model.ItemSelected += (object sender, PersonCollectionAdminPageViewModel.Person e) => this.ItemSelected?.Invoke(this, e);

            BindingContext = model;
        }

        public event EventHandler<PersonCollectionAdminPageViewModel.Person> ItemSelected;
    }
}
