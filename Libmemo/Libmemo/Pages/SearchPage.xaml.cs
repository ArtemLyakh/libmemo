using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPage : ContentPage {

        public SearchPage(string search, EventHandler<Person> OnItemSelected, EventHandler<string> OnSearchChanged) {
            InitializeComponent();

            BindingContext = new SearchPageViewModel(search, OnItemSelected, OnSearchChanged);
        }

    }

}
