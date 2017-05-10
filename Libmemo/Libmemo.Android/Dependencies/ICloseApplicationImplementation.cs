using System;
using Libmemo.Droid;
using Xamarin.Forms;
using Android.App;

[assembly: Dependency(typeof(ICloseApplicationImplementation))]
namespace Libmemo.Droid {
    public class ICloseApplicationImplementation : ICloseApplication {
        public void CloseApplication() {
            var activity = (Activity)Forms.Context;
            activity.FinishAffinity();
        }
    }
}