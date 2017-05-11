using System;
using System.Globalization;
using Xamarin.Forms;

namespace Libmemo {
    public class NullableDateConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            try {
                DateTime? obj = (DateTime?)value;
                if (obj.HasValue) {
                    return obj.Value;
                }
            } catch {
                return DateTime.Now;
            }

            return DateTime.Now;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value;
        }
    }

}
