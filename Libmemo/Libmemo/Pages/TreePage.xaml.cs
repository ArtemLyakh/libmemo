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

        private View GetAddNewButton(Action action) {
            var button = new Image {
                WidthRequest = 75,
                HeightRequest = 75,
                Source = ImageSource.FromResource("Libmemo.Tree.Images.add_button.jpg")
            };
            button.GestureRecognizers.Add(new TapGestureRecognizer() {
                Command = new Command(() => action.Invoke())
            });

            //button.Clicked += (object sender, EventArgs e) => action.Invoke();

            return button;
        }

        private Tuple<View, Point> GetLine(Point a, Point b) {
            var length = Math.Pow(Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.X - b.X, 2), 0.5);
            var rot = Math.Atan((b.X - a.X) / (a.Y - b.Y)) * 180 / Math.PI;

            return Tuple.Create(new BoxView {
                HeightRequest = length,
                WidthRequest = 5,
                BackgroundColor = Color.Black,
                Rotation = rot
            } as View, new Point((a.X + b.X) / 2 - 5 / 2, (a.Y + b.Y) / 2 - length / 2));
        }

        private async void Slider1_ValueChanged(object sender, ValueChangedEventArgs e) {
            await absolute.ScaleTo(e.NewValue / 100);         


            var q = 1;
        }


    }





}