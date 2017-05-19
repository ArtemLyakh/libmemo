using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage {

        public ListView ListView { get { return this.listView; } }


        public ObservableCollection<MenuPageItem> MenuList { get; set; }


        public MenuPage() {
            InitializeComponent();

            MenuList = new ObservableCollection<MenuPageItem>();
            foreach (var item in GetMenuList()) {
                MenuList.Add(item);
            }

            this.BindingContext = this;
        }



        private static IEnumerable<MenuPageItem> GetMenuList() {
            yield return new MenuPageItem {
                Title = "Карта",
                Text = "карта",
                Page = typeof(MapPage)
            };
            yield return new MenuPageItem {
                Title = "Добавить",
                Text = "добавить",
                Page = typeof(AddPage)
            };
            yield return new MenuPageItem {
                Title = "test",
                Action = () => {
                    Device.BeginInvokeOnMainThread(async () => {
                        await App.Current.MainPage.DisplayAlert("Test", "test", "ОК");
                    });
                }
            };
            yield return new MenuPageItem {
                Title = "AuthTest",
                Page = typeof(TestAuthPage)
            };
        }
    }

    public class MenuPageItem {
        public string Title { get; set; }
        public string Text { get; set; }
        public Type Page { get; set; }
        public Action Action { get; set; }
    }
}
