using System;
using SQLite;

namespace Libmemo {
    public interface IDatabaseSavable {
        int Id { get; set; }
        string FIO { get; }
    }

    [Table("Persons")]
    public class Person : IDatabaseSavable {
        [PrimaryKey, Column("_id")]
        public int Id { get; set; }
        public long LastModified { get; set; }
        public int Owner { get; set; }

        public string FIO { get => string.Join(" ", new string[] { LastName, FirstName, SecondName }).Trim(); }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateBirth { get; set; }
        public DateTime? DateDeath { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
        public string ImageUrl { get; set; }
    }

    [Table("Users")]
    public class User : IDatabaseSavable {
        [PrimaryKey, Column("_id")]
        public int Id { get; set; }
        public long LastModified { get; set; }
        public int Owner { get; set; }

        public string FIO { get => string.Join(" ", new string[] { LastName, FirstName, SecondName }); }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateBirth { get; set; }
        public string Icon { get; set; }
        public string ImageUrl { get; set; }
    }
}
