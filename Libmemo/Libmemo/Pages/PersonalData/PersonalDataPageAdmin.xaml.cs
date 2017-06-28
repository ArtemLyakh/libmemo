using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PersonalDataPageAdmin : ContentPage {
        public PersonalDataPageAdmin(int id) {
            InitializeComponent();
            BindingContext = new PersonalDataPageAdminViewModel(id);
        }
    }
}
