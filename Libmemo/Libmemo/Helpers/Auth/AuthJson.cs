using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo {
    class AuthJson {
        public int id { get; set; }
        public bool is_admin { get; set; }
        public int? person_id { get; set; }
        public string fio { get; set; }
        public string email { get; set; }
    }
}
