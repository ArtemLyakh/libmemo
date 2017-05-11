using System;
using System.Globalization;
using Xamarin.Forms;

namespace Libmemo {
    public class NullableDoubleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            try {
                Double? obj = (Double?)value;
                if (obj.HasValue) {
                    return obj.Value.ToString(CultureInfo.InvariantCulture);
                }
            } catch {
                return string.Empty;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {

            double res;

            if (double.TryParse((string)value, NumberStyles.Float, CultureInfo.InvariantCulture, out res)
                || double.TryParse((string)value, NumberStyles.Float, CultureInfo.CurrentCulture, out res)
                ) {
                return res;
            }

            return null;
        }
    }

}
