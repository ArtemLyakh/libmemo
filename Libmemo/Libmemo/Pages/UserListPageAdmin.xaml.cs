using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserListPageAdmin : ContentPage {
        public UserListPageAdmin() {
            InitializeComponent();

            var model = new UserListAdminPageViewModel();
            model.ItemSelected += (object sender, UserListAdminPageViewModel.User e) => this.ItemSelected?.Invoke(this, e);

            BindingContext = model;
        }
 
        public event EventHandler<UserListAdminPageViewModel.User> ItemSelected;

    }
}