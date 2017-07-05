using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectPersonExceptPage : ContentPage {
        public SelectPersonExceptPage(IEnumerable<int> except) {
            InitializeComponent();

            var model = new SelectPersonExceptPageViewModel(except);
            model.ItemSelected += (sender, selected) => this.ItemSelected?.Invoke(this, selected.Person);

            this.BindingContext = model;
        }

        public event EventHandler<Person> ItemSelected;

        protected override void OnAppearing() {
            base.OnAppearing();
            ((SelectPersonExceptPageViewModel)BindingContext).LoadCommand.Execute(null);
        }

    }
}
