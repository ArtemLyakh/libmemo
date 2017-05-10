using Xamarin.Forms;

namespace Libmemo {
    public class ToastNotificator {
        public ToastNotificator() { }

        public void Show(string text) {
            DependencyService.Get<IToastNotification>().Show(text);
        }
    }
}
