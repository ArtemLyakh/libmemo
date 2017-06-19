using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Libmemo {
    public class CustomScrollView : ScrollView {
        public static readonly BindableProperty MaximumZoomProperty = BindableProperty.Create(nameof(MaximumZoom), typeof(double), typeof(CustomScrollView), 1d);

        public double MaximumZoom {
            get { return (double)GetValue(MaximumZoomProperty); }
            set { SetValue(MaximumZoomProperty, value); }
        }

        public static readonly BindableProperty MinimumZoomProperty = BindableProperty.Create(nameof(MinimumZoom), typeof(double), typeof(CustomScrollView), 1d);

        public double MinimumZoom {
            get { return (double)GetValue(MinimumZoomProperty); }
            set { SetValue(MinimumZoomProperty, value); }
        }
    }
}
