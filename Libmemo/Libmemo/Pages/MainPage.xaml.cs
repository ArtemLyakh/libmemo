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

            this.menuPage.ListView.ItemSelected += OnMenuItemSelected;
        }

        private NavigationPage NavStack {
            get => (NavigationPage)this.Detail;
        }

        public async Task PopToRootPage() => await NavStack.Navigation.PopToRootAsync();
        public async  Task Pop() => await NavStack.Navigation.PopAsync();
        public async Task Push(Page page) => await NavStack.Navigation.PushAsync(page);
        public async Task PushRoot(Page page) {
            await PopToRootPage();
            await NavStack.Navigation.PushAsync(page);
        }





        private void OnMenuItemSelected(object sender, SelectedItemChangedEventArgs e) {
            var item = e.SelectedItem as MenuItem;
            if (item != null) {
                this.menuPage.ListView.SelectedItem = null;
                IsPresented = false;

                if (item.Action != null) {
                    item.Action.Invoke();
                }

                //if (item.Page != null) {
                //    Detail = new NavigationPage((Page)Activator.CreateInstance(item.Page));
                //}
            }
        }

        public void ExecuteMenuItem(MenuItem item) {
            this.OnMenuItemSelected(null, new SelectedItemChangedEventArgs(item));
        }









    }
}
