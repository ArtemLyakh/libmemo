using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage {
        public MainPage() {
            InitializeComponent();

            (Master as MenuPage).ListView.ItemSelected += OnMenuItemSelected;
        }

        private NavigationPage NavStack {
            get => (NavigationPage)this.Detail;
        }

        public async Task PopToRootPage() => await NavStack.Navigation.PopToRootAsync();
        public async Task Pop() => await NavStack.Navigation.PopAsync();
        public async Task Push(Page page) => await NavStack.Navigation.PushAsync(page);
        public async Task PushRoot(Page page) {
            var root = NavStack.Navigation.NavigationStack.First();

            foreach (var item in NavStack.Navigation.NavigationStack.Where(i => i != root)) {
				NavStack.Navigation.RemovePage(item);
			}

            await NavStack.Navigation.PushAsync(page);
        }
        public Task ReplaceCurrentPage(Page page) {
            var last = NavStack.Navigation.NavigationStack.Last();
            NavStack.Navigation.RemovePage(last);
            return NavStack.Navigation.PushAsync(page);
        }




        private async void OnMenuItemSelected(object sender, SelectedItemChangedEventArgs e) {
            var item = e.SelectedItem as MenuPage.MenuItem;
            if (item != null) {
                (Master as MenuPage).ListView.SelectedItem = null;

                await item.Action?.Invoke();
                IsPresented = false;
            }
        }




    }
}
