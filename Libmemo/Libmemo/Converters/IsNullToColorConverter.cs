using System;
using System.Globalization;
using Xamarin.Forms;

namespace Libmemo {
    public class IsNullToColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null) {
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
