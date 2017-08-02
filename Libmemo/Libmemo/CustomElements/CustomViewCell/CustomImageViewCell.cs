using FFImageLoading.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Libmemo {
    public class CustomImageViewCell : ViewCell {

        readonly CachedImage cachedImage = null;
        readonly Label label = null;

        public CustomImageViewCell() {
            var stack = new StackLayout {
                Orientation = StackOrientation.Horizontal,
                Padding = new Thickness(10, 5),
                HeightRequest = 70,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            this.cachedImage = new CachedImage {
                DownsampleToViewSize = true,
                HeightRequest = 60,
                WidthRequest = 60,
                Aspect = Aspect.AspectFill,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.Start
            };
            stack.Children.Add(this.cachedImage);
            this.label = new Label {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                TextColor = Color.Black,
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0)
            };
            stack.Children.Add(this.label);

            View = stack;
        }

        protected override void OnBindingContextChanged() {
            this.cachedImage.Source = null;
            this.label.Text = null;

            var item = BindingContext as ListElement.ImageElement;
            if (item == null) return;

            cachedImage.Source = item.Image;
            label.Text = item.FIO;

            base.OnBindingContextChanged();
        }
    }
}
