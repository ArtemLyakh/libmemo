using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo {
    public interface IFileStreamPicker {
        Stream GetStream(string path);
        byte[] GetResizedJpeg(string path, int width, int height);
        Task<byte[]> GetResizedJpegAsync(string path, int width, int height);
    }
}
