using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage {

        public ListView ListView { get { return this.listView; } }

        public MenuPage() {
            InitializeComponent();

            ListView.ItemsSource = new List<MenuPageItem> {
                new MenuPageItem {
                    Title = "Карта",
                    Text = "карта",
                    Page = typeof(MapPage)
                },
                new MenuPageItem {
                    Title = "Добавить",
                    Text = "добавить",
                    Page = typeof(AddPage)
                },
                new MenuPageItem {
                    Title = "test",
                    Action = () => {
                        Device.BeginInvokeOnMainThread(async () => {
                            await App.Current.MainPage.DisplayAlert("Test", "test", "ОК");
                        });
                    }
                }
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
