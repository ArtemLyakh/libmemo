using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Libmemo {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddPage : ContentPage {
        public AddPage(Position userPosition, EventHandler OnItemAdded = null) {
            InitializeComponent();
            BindingContext = new AddPageViewModel(userPosition, OnItemAdded);
        }
    }

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
