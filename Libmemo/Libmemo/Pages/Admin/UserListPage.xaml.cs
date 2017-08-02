using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserListPage : ContentPage {
        public UserListPage() {
            InitializeComponent();
            var model = new UserListPageViewModel();
            model.ItemSelected += (sender, e) => this.ItemSelected?.Invoke(this, e);
            BindingContext = model;
        }

        public event EventHandler<ListElement.TextElement> ItemSelected;

        protected override void OnAppearing() {
            base.OnAppearing();
            ((UserListPageViewModel)BindingContext).LoadCommand.Execute(null);
        }

    }
}