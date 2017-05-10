using System.Threading.Tasks;
using Xamarin.Forms;

namespace Libmemo {
    public interface IImageFileToByteArrayConverter {
        Task<byte[]> Get(ImageSource source);
    }
}