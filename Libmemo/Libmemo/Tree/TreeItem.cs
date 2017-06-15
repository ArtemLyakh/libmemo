using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;

namespace Libmemo.Tree {
    public class TreeItem : ContentView {

        public event EventHandler<int> DeleteClicked;

        public TreeItem(IDatabaseSavable person) {
            var stack = new StackLayout {
                HeightRequest = 200,
                WidthRequest = 100
            };

            var stackTop = new StackLayout {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = 15
            };
            var deleteButton = new Button {
                Text = char.ConvertFromUtf32(0x2716),
                FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Button)),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            deleteButton.Clicked += (object sender, EventArgs e) => DeleteClicked?.Invoke(this, person.Id);
            stackTop.Children.Add(deleteButton);
            stack.Children.Add(stackTop);

            var image = new Image {
                HeightRequest = 100,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Source = string.IsNullOrWhiteSpace(person.Icon) ? null : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(person.Icon)))
            };
            stack.Children.Add(image);

            var fio = new Label {
                Text = person.FIO,
                HeightRequest = 85,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.WordWrap
            };
            stack.Children.Add(fio);



            Content = stack;
        }

    }
}
