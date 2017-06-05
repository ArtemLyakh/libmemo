using System.Collections.Generic;

namespace Libmemo {
    public class JsonData {
        public List<UserJsonAdd> users { get; set; }
        public List<PersonJsonAdd> persons { get; set; }
        public List<PersonJsonDelete> delete { get; set; }
    }

    public class UserJsonAdd {
        public int id { get; set; }
        public long modified { get; set; }
        //public string fio { get; set; }
        public string first_name { get; set; }
        public string second_name { get; set; }
        public string last_name { get; set; }
        public string date_birth { get; set; }
        public string icon { get; set; }
        public string image_url { get; set; }
    }

    public class PersonJsonAdd {
        public int id { get; set; }
        public long modified { get; set; }
        //public string fio { get; set; }
        public string first_name { get; set; }
        public string second_name { get; set; }
        public string last_name { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string date_birth { get; set; }
        public string date_death { get; set; }
        public string icon { get; set; }
        public string image_url { get; set; }
        public string text { get; set; }
    }

    public class PersonJsonDelete {
        public int id { get; set; }
        public long modified { get; set; }
    }
}
