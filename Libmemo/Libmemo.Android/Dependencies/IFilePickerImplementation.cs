using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Libmemo.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(IFilePickerImplementation))]
namespace Libmemo.Droid {
    public class IFilePickerImplementation : IFileStreamPicker {
        public Stream GetStream(string path) {
            return File.Open(path, FileMode.Open);
        }
    }
}