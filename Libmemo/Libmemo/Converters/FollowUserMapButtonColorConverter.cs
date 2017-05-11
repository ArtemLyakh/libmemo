using System;
using System.Globalization;
using Xamarin.Forms;

namespace Libmemo {

    public class FollowUserMapButtonColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((bool)value) {
                return (Color)parameter;
            } else {
                return Xamarin.Forms.Color.Default;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

}
