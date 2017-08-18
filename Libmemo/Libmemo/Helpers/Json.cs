﻿using System;
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

    public class Register {
        public Auth auth { get; set; }
        public PersonJson.Update person { get; set; }
    }

    public class User {
        public int id { get; set; }
        public string fio { get; set; }
        public string email { get; set; }
    }

    public class PersonalData {
        public string first_name { get; set; }
        public string second_name { get; set; }
        public string last_name { get; set; }
        public string date_birth { get; set; }
        public string photo_url { get; set; }
    }
}
