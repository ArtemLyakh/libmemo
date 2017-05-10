using Android.Widget;
using Xamarin.Forms;
using Libmemo.Droid;

[assembly: Dependency(typeof(IToastNotificationsImplementation))]
namespace Libmemo.Droid {
    public class IToastNotificationsImplementation : IToastNotification {

        public void Show(string text) {
            Toast.MakeText(Forms.Context, text, ToastLength.Short).Show();
        }
    }
}
