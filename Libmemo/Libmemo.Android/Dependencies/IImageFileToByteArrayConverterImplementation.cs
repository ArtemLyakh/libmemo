using System.IO;
using System.Threading.Tasks;
using Libmemo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: Dependency(typeof(IImageFileToByteArrayConverterImplementation))]
namespace Libmemo.Droid {
    class IImageFileToByteArrayConverterImplementation : IImageFileToByteArrayConverter {
        public async Task<byte[]> Get(ImageSource source) {
            var bitmap = await new FileImageSourceHandler().LoadImageAsync(source, null);
            byte[] bitmapData;
            using (var stream = new MemoryStream()) {
                bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, 75, stream);
                bitmapData = stream.ToArray();
            }
            return bitmapData;
        }
    }
}