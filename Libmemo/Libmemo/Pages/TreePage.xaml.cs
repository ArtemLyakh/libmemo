using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TreePage : ContentPage {
        public TreePage() {
            InitializeComponent();
            Init();
        }

        private void Init() {
            absolute.Children.Clear();
            absolute.WidthRequest = 1000;
            absolute.HeightRequest = 1000;

            BoxView b1 = new BoxView {
                BackgroundColor = Color.Red,
                HeightRequest = 100,
                WidthRequest = 200,
            };
            absolute.Children.Add(b1, new Point(10, 10));

            BoxView b2 = new BoxView {
                BackgroundColor = Color.Green,
                HeightRequest = 100,
                WidthRequest = 200
            };
            absolute.Children.Add(b2, new Point(50, 200));

            BoxView b3 = new BoxView {
                BackgroundColor = Color.Blue,
                HeightRequest = 100,
                WidthRequest = 200
            };
            absolute.Children.Add(b3, new Point(100, 400));

            absolute.Children.Add(GetButton(), new Point(250, 250));
        }


        private View GetButton() {
            var b = new Button();
            b.Text = "text";
            b.Clicked += B_Clicked;
            return b;
        }

        private void B_Clicked(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void Search_Button_Clicked(object sender, EventArgs e) {

        }

        private void Reset_Button_Clicked(object sender, EventArgs e) {

        }
        
    }
}