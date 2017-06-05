using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetailPage : ContentPage {

        public DetailPage(Person person) {
            InitializeComponent();
            this.BindingContext = new DetailPageViewModel(person);
        }

    }
}
