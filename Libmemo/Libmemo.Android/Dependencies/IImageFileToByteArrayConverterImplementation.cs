using System.IO;
using System.Threading.Tasks;
using Libmemo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Graphics;

[assembly: Dependency(typeof(IImageFileToByteArrayConverterImplementation))]
namespace Libmemo.Droid {
    class IImageFileToByteArrayConverterImplementation : IImageFileToByteArrayConverter {
        public async Task<byte[]> Get(ImageSource source) {
            var file = ((FileImageSource)source).File;

            BitmapFactory.Options options = await GetBitmapOptionsOfImageAsync(file);
            Bitmap bitmap = await LoadScaledDownBitmapAsync(file, options, 1000, 1000);

            using (var stream = new MemoryStream()) {
                bitmap.Compress(Bitmap.CompressFormat.Jpeg, 75, stream);
                return stream.ToArray();
            }

        }

        private async Task<BitmapFactory.Options> GetBitmapOptionsOfImageAsync(string file) {
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };

            // The result will be null because InJustDecodeBounds == true.
            Bitmap result = await BitmapFactory.DecodeFileAsync(file, options);

            int imageHeight = options.OutHeight;
            int imageWidth = options.OutWidth;

            return options;
        }

        private static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight) {
            // Raw height and width of image
            float height = options.OutHeight;
            float width = options.OutWidth;
            double inSampleSize = 1D;

            if (height > reqHeight || width > reqWidth) {

                // Calculate a inSampleSize that is a power of 2 - the decoder will use a value that is a power of two anyway.
                while ((height / inSampleSize) * (width / inSampleSize) > reqWidth * reqHeight) {
                    inSampleSize *= 2;
                }
            }

            return (int)inSampleSize;
        }

        private async Task<Bitmap> LoadScaledDownBitmapAsync(string file, BitmapFactory.Options options, int reqWidth, int reqHeight) {
            // Calculate inSampleSize
            options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);

            // Decode bitmap with inSampleSize set
            options.InJustDecodeBounds = false;

            return await BitmapFactory.DecodeFileAsync(file, options);
        }
    }
}