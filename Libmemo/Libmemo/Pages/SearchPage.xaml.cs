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

        public SearchPage(IEnumerable<IDatabaseSavable> data, string search = null) {
            InitializeComponent();

            var model = new SearchPageViewModel(data, search);
            model.ItemSelected += (sender, id) => this.ItemSelected?.Invoke(this, id);
            model.SearchTextChanged += (sender, text) => this.SearchTextChanged?.Invoke(this, text);

            this.BindingContext = model;
        }

        public event EventHandler<int> ItemSelected;
        public event EventHandler<string> SearchTextChanged;

    }

}
