using System.Collections.Generic;

namespace Libmemo {
    public class PersonJson {
        public List<Update> update { get; set; }
        public List<Delete> delete { get; set; }

        public class Update {
            public string type { get; set; }
            public int? id { get; set; }
            public long? modified { get; set; }
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
        }

        public class Delete {
            public int? id { get; set; }
            public long? modified { get; set; }
        }
    }




}
