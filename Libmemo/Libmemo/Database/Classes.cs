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
        public class PersonDB {
            [PrimaryKey, Column("_id")]
            public int _Id { get; set; }
            public PersonType _PersonType { get; set; }
            public long _LastModified { get; set; }
            public int _Owner { get; set; }
            public string _FirstName { get; set; }
            public string _SecondName { get; set; }
            public string _LastName { get; set; }
            public DateTime? _DateBirth { get; set; }
            public DateTime? _DateDeath { get; set; }
            public double _Latitude { get; set; }
            public double _Longitude { get; set; }
            public string _Text { get; set; }
            public string _IconUrl { get; set; }
            public string _ImageUrl { get; set; }
            public double? _Height { get; set; }
            public double? _Width { get; set; }
            public string _SchemeUrl { get; set; }
        }

        public int Id { get; set; }
        public PersonType PersonType { get; set; }
        public long LastModified { get; set; }
        public int Owner { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string LastName { get; set; }
        public string FIO => string.Join(" ", new string[] { LastName, FirstName, SecondName }).Trim();
        public DateTime? DateBirth { get; set; }
        public DateTime? DateDeath { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Text { get; set; }
        public Uri IconUrl { get; set; }
        public Uri ImageUrl { get; set; }
        public double? Height { get; set; }
        public double? Width { get; set; }
        public Uri SchemeUrl { get; set; }

        public static PersonDB ConvertToDatabase(Person person) => person == null ? null : new PersonDB {
           _DateBirth = person.DateBirth,
           _DateDeath = person.DateDeath,
           _FirstName = person.FirstName,
           _Height = person.Height,
           _IconUrl = person.IconUrl?.ToString(),
           _Id = person.Id,
           _ImageUrl = person.ImageUrl?.ToString(),
           _LastModified = person.LastModified,
           _LastName = person.LastName,
           _Latitude = person.Latitude,
           _Longitude = person.Longitude,
           _Owner = person.Owner,
           _PersonType = person.PersonType,
           _SchemeUrl = person.SchemeUrl?.ToString(),
           _SecondName = person.SecondName,
           _Text = person.Text,
           _Width = person.Width
        };


        public static Person ConvertFromDatabase(PersonDB personDb) => personDb == null ? null : new Person {
            DateBirth = personDb._DateBirth,
            DateDeath = personDb._DateDeath,
            FirstName = personDb._FirstName,
            Height = personDb._Height,
            IconUrl = Uri.TryCreate(personDb._IconUrl, UriKind.Absolute, out Uri iconUrl) ? iconUrl : null,
            Id = personDb._Id,
            ImageUrl = Uri.TryCreate(personDb._ImageUrl, UriKind.Absolute, out Uri imageUrl) ? imageUrl : null,
            LastModified = personDb._LastModified,
            LastName = personDb._LastName,
            Latitude = personDb._Latitude,
            Longitude = personDb._Longitude,
            Owner = personDb._Owner,
            PersonType = personDb._PersonType,
            SchemeUrl = Uri.TryCreate(personDb._SchemeUrl, UriKind.Absolute, out Uri schemeUrl) ? schemeUrl : null,
            SecondName = personDb._SecondName,
            Text = personDb._Text,
            Width = personDb._Width
        };


    }


}
