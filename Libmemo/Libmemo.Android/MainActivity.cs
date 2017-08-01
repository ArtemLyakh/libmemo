using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Libmemo.Droid {
    [Activity(Label = "Libmemo", Icon = "@drawable/icon", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
        protected override void OnCreate(Bundle bundle) {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            global::Xamarin.FormsMaps.Init(this, bundle);
            FFImageLoading.Forms.Droid.CachedImageRenderer.Init();
            ImageCircle.Forms.Plugin.Droid.ImageCircleRenderer.Init();
            LoadApplication(new App());
        }

        protected override void OnDestroy() {
            App.TextToSpeech.Stop();
            base.OnDestroy();
        }

        protected override void OnStop() {
            App.TextToSpeech.Stop();
            base.OnStop();
        }

        protected override void OnPause() {
            App.TextToSpeech.Stop();
            base.OnPause();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

