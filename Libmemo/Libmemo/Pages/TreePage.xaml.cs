using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TreePage : ContentPage {
        private Tree Tree { get; set; }

        public TreePage(int? id = null) {
            InitializeComponent();
            Init(id);

        }

        private async void Init(int? id) {
            if (!AuthHelper.IsAdmin) id = AuthHelper.CurrentUserId;
            if (!id.HasValue) {
                await App.GlobalPage.PopToRootPage();
                return;
            }

            Tree = new Tree(id.Value);
            try {
                var success = await Tree.LoadData();
                if (!success) throw new Exception();
                Tree.DrawTree(absolute);

            } catch (UnauthorizedAccessException) {
                await App.GlobalPage.PopToRootPage();
                return;
            } catch (FormatException) {
                Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", "Данные повреждены", "Ок"));
                await App.GlobalPage.PopToRootPage();
                return;
            } catch {
                var q = 1;
            }
            
        }






        //private async void Init() {



        //    db = Enumerable.Empty<IDatabaseSavable>()
        //            .Concat(await App.Database.GetItems<User>())
        //            .Concat(await App.Database.GetItems<Person>());

        //    absolute.WidthRequest = 1500;
        //    absolute.HeightRequest = 1500;


        //    TreeItem user = new TreeItem(db.ElementAt(5)) {
        //        Mother = new TreeItemView(db.ElementAt(1)),
        //        Father = new TreeItemView(db.ElementAt(2))
        //    };

        //    var item = GetElementView(user.Current);
        //    absolute.Children.Add(item, new Point(200, 200));

        //    var btn = GetAddNewButton(() => AddNewItem());
        //    absolute.Children.Add(btn, new Point(400, 225));



        //    var p1 = new Point(500, 500);
        //    var p2 = new Point(600, 400);
        //    absolute.Children.Add(new BoxView {
        //        WidthRequest = 5,
        //        HeightRequest = 5,
        //        BackgroundColor = Color.Blue
        //    }, p1);
        //    absolute.Children.Add(new BoxView {
        //        WidthRequest = 5,
        //        HeightRequest = 5,
        //        BackgroundColor = Color.Blue
        //    }, p2);

        //    var line = new BoxView {
        //        WidthRequest = 50,
        //        HeightRequest = 50,
        //        BackgroundColor = Color.Green
        //    };
        //    absolute.Children.Add(line, new Point(500, 500));


        //    var q = 1;
        //}

        //private void AddNewItem() {
        //    var item = GetElementView(db.ElementAt(10));
        //    absolute.Children.Add(item, new Point(400, 400));
        //}





        private void Search_Button_Clicked(object sender, EventArgs e) {

        }

        private void Reset_Button_Clicked(object sender, EventArgs e) {
            var q = 1;
        }
        








        private void DeleteItem(int id) {
            var q = 1;
        }







        //private View GetElementView(IDatabaseSavable person) {
        //    var stack = new StackLayout {
        //        HeightRequest = 150,
        //        WidthRequest = 100,
        //        BackgroundColor = Color.LightGray
        //    };

        //    var stackTop = new StackLayout {
        //        HorizontalOptions = LayoutOptions.FillAndExpand,
        //        Orientation = StackOrientation.Horizontal,
        //        HeightRequest = 25,
        //    };
        //    var deleteButton = new Button {
        //        Text = "x",
        //        BackgroundColor = Color.Red,
        //        TextColor = Color.White,
        //        FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Button)),
        //        HorizontalOptions = LayoutOptions.EndAndExpand,
        //        HeightRequest = 25,
        //        WidthRequest = 25
        //    };
        //    deleteButton.Clicked += (object sender, EventArgs e) => DeleteItem(person.Id);
        //    stackTop.Children.Add(deleteButton);
        //    stack.Children.Add(stackTop);

        //    var image = new Image {
        //        HeightRequest = 75,
        //        HorizontalOptions = LayoutOptions.FillAndExpand,
        //        Source = string.IsNullOrWhiteSpace(person.Icon) 
        //            ? ImageSource.FromResource("Libmemo.Tree.Images.no_photo.jpg") 
        //            : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(person.Icon)))
        //    };
        //    stack.Children.Add(image);

        //    var fio = new Label {
        //        Text = person.FIO,
        //        HeightRequest = 50,
        //        FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
        //        HorizontalOptions = LayoutOptions.FillAndExpand,
        //        HorizontalTextAlignment = TextAlignment.Center,
        //        VerticalTextAlignment = TextAlignment.Center,
        //        LineBreakMode = LineBreakMode.WordWrap
        //    };
        //    stack.Children.Add(fio);

        //    return stack;
        //}

      
        private async void Slider1_ValueChanged(object sender, ValueChangedEventArgs e) {
            await absolute.ScaleTo(e.NewValue / 100);         


            var q = 1;
        }


    }





}