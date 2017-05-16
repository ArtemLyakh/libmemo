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
                    TargetType = typeof(MapPage)
                },
                new MenuPageItem {
                    Title = "Добавить",
                    Text = "добавить",
                    TargetType = typeof(AddPage)
                }
            };
        }
    }

    internal class MenuPageItem {
        public string Title { get; set; }
        public string Text { get; set; }
        public Type TargetType { get; set; }
    }
}
