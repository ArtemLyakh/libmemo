using System.Collections.Generic;

namespace Libmemo {
    public class JsonData {
        public List<PersonJsonAdd> add { get; set; }
        public List<PersonJsonDelete> delete { get; set; }
    }

    public class PersonJsonAdd {
        public int id { get; set; }
        public long modified { get; set; }
        public bool active { get; set; }
        public string fio { get; set; }
        public string link { get; set; }
        public string date_birth { get; set; }
        public string date_death { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string text { get; set; }
        public string image { get; set; }
    }

    public class PersonJsonDelete {
        public int id { get; set; }
        public long modified { get; set; }
    }
}
