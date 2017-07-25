using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo.Json {
    public class Auth {
        public int id { get; set; }
        public bool is_admin { get; set; }
        public string fio { get; set; }
        public string email { get; set; }
    }
    public class Message {
        public string message { get; set; }
    }
}
