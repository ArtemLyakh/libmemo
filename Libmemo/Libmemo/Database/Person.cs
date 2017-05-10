using System;
using SQLite;

namespace Libmemo {
    [Table("Persons")]
    public class Person {
        [PrimaryKey, Column("_id")]
        public int Id { get; set; }
        public long LastModified { get; set; }

        public string Name { get; set; }
        public string Link { get; set; }
        public DateTime? DateBirth { get; set; }
        public DateTime? DateDeath { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Text { get; set; }
        public string Image { get; set; }
    }
}
