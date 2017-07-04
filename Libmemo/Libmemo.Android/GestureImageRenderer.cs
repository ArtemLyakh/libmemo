using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Libmemo.Droid;
using Xamarin.Forms;
using Libmemo;
using Xamarin.Forms.Platform.Android;

[assembly:ExportRenderer (typeof(LongTapImage), typeof(GestureImageRenderer))]
namespace Libmemo.Droid {
    class GestureImageRenderer : ImageRenderer {

        LongTapImage view;
        private LongTapImage.IRenderable Renderable => view as LongTapImage.IRenderable;

        public GestureImageRenderer () : base() {
            this.LongClickable = true;
            this.LongClick += (sender, e) => {
                if (e.Handled) Renderable?.LongTapGestureHandler();
            };
            this.Click += (sender, e) => Renderable?.TapGestureHandler();
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Image> e) {
            base.OnElementChanged(e);
            view = (LongTapImage)e.NewElement;
        }

    }
}