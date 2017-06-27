using System;
using SQLite;

namespace Libmemo {

    public interface IDatabaseSavable {
        int Id { get; set; }
        string FIO { get; set; }
    }
    public class User {
        public int Id { get; set; }
        public PersonType PersonType { get; set; }
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
        public double? Height { get; set; }
        public double? Width { get; set; }
        private string SchemeUrl { get; set; }

    }



    public enum PersonType {
        Dead, Alive
    }

    [Table("Persons")]
    public class Person {
        [PrimaryKey, Column("_id")]
        public int Id { get; set; }
        public PersonType PersonType { get; set; }
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
        private string _ImageUrl { get; set; }
        [Ignore]
        public Uri ImageUrl {
            get => Uri.TryCreate(_ImageUrl, UriKind.Absolute, out Uri uri) ? uri : null;
            set => _ImageUrl = value.ToString();
        }
        public double? Height { get; set; }
        public double? Width { get; set; }
        private string _SchemeUrl { get; set; }
        [Ignore]
        public Uri SchemeUrl {
            get => Uri.TryCreate(_SchemeUrl, UriKind.Absolute, out Uri uri) ? uri : null;
            set => _SchemeUrl = value.ToString();
        }
    }


}
