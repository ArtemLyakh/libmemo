using System;
using System.Globalization;
using Xamarin.Forms;

namespace Libmemo {
    public class IsPinSpeakableConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null && value.GetType() == typeof(CustomPin)) {
                if (((CustomPin)value).PinImage == PinImage.Speaker) {
                    return true;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

}
