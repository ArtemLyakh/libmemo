using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
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



		public byte[] GetResizedJpeg(string path, int width, int height)
		{
            Bitmap originalImage;

            using (var stream = File.Open(path, FileMode.Open)) {
                originalImage = BitmapFactory.DecodeStream(stream);
            }

            var resultWidth = originalImage.Width;
            var resultHeight = originalImage.Height;
            if (resultWidth > width) {
				resultHeight = resultHeight * width / resultWidth;
                resultWidth = width;
            }
            if (resultHeight > height) {
				resultWidth = resultWidth * height / resultHeight;
                resultHeight = height;
            }

			Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, resultWidth, resultHeight, false);

            byte[] arr = null;

            using (var ms = new MemoryStream()) {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 75, ms);
                arr = ms.ToArray();
            }

            return arr;
		}

        public Task<byte[]> GetResizedJpegAsync(string path, int width, int height) =>
            Task.Run(() => GetResizedJpeg(path, width, height));


    }
}