using System;
using SQLite;

namespace Libmemo {
    public interface IDatabaseSavable {
        int Id { get; set; }
        string Name { get; set; }
    }

    [Table("Persons")]
    public class Person : IDatabaseSavable {
        [PrimaryKey, Column("_id")]
        public int Id { get; set; }
        public long LastModified { get; set; }

        public string Name { get; set; }
        public DateTime? DateBirth { get; set; }
        public DateTime? DateDeath { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Text { get; set; }
        public string Image { get; set; }
    }

    [Table("Users")]
    public class User : IDatabaseSavable {
        [PrimaryKey, Column("_id")]
        public int Id { get; set; }
        public long LastModified { get; set; }

        public string Name { get; set; }
        public DateTime? DateBirth { get; set; }
        public string Image { get; set; }
    }
}
