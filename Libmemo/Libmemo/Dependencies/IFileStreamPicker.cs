using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo {
    public interface IFileStreamPicker {
        Stream GetStream(string path);
    }
}
