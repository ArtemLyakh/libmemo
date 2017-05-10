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

    public class ListView : Xamarin.Forms.ListView {
        public static BindableProperty ItemClickCommandProperty = BindableProperty.Create<ListView, ICommand>(x => x.ItemClickCommand, null);

        public ListView() {
            this.ItemTapped += this.OnItemTapped;
        }


        public ICommand ItemClickCommand {
            get { return (ICommand)this.GetValue(ItemClickCommandProperty); }
            set { this.SetValue(ItemClickCommandProperty, value); }
        }

        private object prevTapped = null;
        private void OnItemTapped(object sender, ItemTappedEventArgs e) {

            if (this.prevTapped != e.Item) {
                this.prevTapped = e.Item;
                return;
            } else {
                if (e.Item != null && this.ItemClickCommand != null && this.ItemClickCommand.CanExecute(e)) {
                    this.ItemClickCommand.Execute(e.Item);
                    this.SelectedItem = null;
                    this.prevTapped = null;

                }
            }

        }
    }
}
