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
    public partial class MapSearchPage : ContentPage {

        public MapSearchPage(string search = null) {
            InitializeComponent();

            var model = new SearchPageViewModel(search);
            model.ItemSelected += (sender, person) => this.ItemSelected?.Invoke(this, person);
            model.SearchChanged += (sender, text) => this.SearchTextChanged?.Invoke(this, text);

            this.BindingContext = model;
        }

        public event EventHandler<SearchPageViewModel.Person> ItemSelected;
        public event EventHandler<string> SearchTextChanged;

        protected override bool OnBackButtonPressed() {
            if (((SearchPageViewModel)BindingContext).IsPopupVisible) {
                (((SearchPageViewModel)BindingContext).IsPopupVisible) = false;
                return true;
            }

            return base.OnBackButtonPressed();
        }

    }

}
