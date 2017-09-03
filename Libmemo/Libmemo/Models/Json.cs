using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo.Json {
    public class Person {
        public string type { get; set; }
        public int? id { get; set; }
        public int? owner { get; set; }

        public string first_name { get; set; }
        public string second_name { get; set; }
        public string last_name { get; set; }
        public string date_birth { get; set; }
        public string icon_url { get; set; }
        public string photo_url { get; set; }
        public string small_photo_url { get; set; }

        public string latitude { get; set; }
        public string longitude { get; set; }
        public string date_death { get; set; }
        public string text { get; set; }
        public string height { get; set; }
        public string width { get; set; }
        public string scheme_url { get; set; }

        public string address { get; set; }
        public string city { get; set; }

        public string section { get; set; }
        public string grave_number { get; set; }
    }

    public class Error
    {
        public string error { get; set; }
    }
}
