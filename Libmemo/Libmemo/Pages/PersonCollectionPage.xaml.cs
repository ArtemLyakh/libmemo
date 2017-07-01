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

            var model = new PersonCollectionPageViewModel();
            model.ItemSelected += (object sender, PersonCollectionPageViewModel.Person e) => this.ItemSelected?.Invoke(this, e);

            BindingContext = model;
        }

        public event EventHandler<PersonCollectionPageViewModel.Person> ItemSelected;

        protected override void OnAppearing() {
            base.OnAppearing();
            ((PersonCollectionPageViewModel)BindingContext).LoadCommand.Execute(null);
        }

    }
}
