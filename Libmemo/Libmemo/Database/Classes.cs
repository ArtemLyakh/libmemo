using System;
using SQLite;
using System.Globalization;

namespace Libmemo {


    public enum PersonType {
        Dead, Alive, User
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



        public static Person ConvertFromJson(PersonJson.Update json) {
            var person = new Person();

            switch (json.type) {
                case "a": person.PersonType = PersonType.Alive; break;
                case "d": person.PersonType = PersonType.Dead; break;
                case "u": person.PersonType = PersonType.User; break;
                default: return null;
            }

            if (json.id.HasValue) person.Id = json.id.Value;
            else return null;

            if (json.modified.HasValue) person.LastModified = json.modified.Value;
            else return null;

            if (json.owner.HasValue) person.Owner = json.owner.Value;
            else return null;


            if (!string.IsNullOrWhiteSpace(json.first_name)) person.FirstName = json.first_name;
            else return null;

            if (!string.IsNullOrWhiteSpace(json.second_name)) person.SecondName = json.second_name;

            if (!string.IsNullOrWhiteSpace(json.last_name)) person.LastName = json.last_name;

            if (DateTime.TryParse(json.date_birth, out DateTime dBirth)) person.DateBirth = dBirth;

            if (!string.IsNullOrWhiteSpace(json.icon_url) && Uri.TryCreate(json.icon_url, UriKind.Absolute, out Uri iconUrl))
                person.IconUrl = iconUrl;

            if (!string.IsNullOrWhiteSpace(json.photo_url) && Uri.TryCreate(json.photo_url, UriKind.Absolute, out Uri photoUrl))
                person.ImageUrl = photoUrl;


            if (person.PersonType == PersonType.Dead) {
                if (double.TryParse(json.latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double latitude)) {
                    person.Latitude = latitude;
                } else return null;

                if (double.TryParse(json.longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double longitude)) {
                    person.Longitude = longitude;
                } else return null;

                if (DateTime.TryParse(json.date_death, out DateTime dDeath)) person.DateDeath = dDeath;

                if (!string.IsNullOrWhiteSpace(json.text)) person.Text = json.text;

                if (double.TryParse(json.height, NumberStyles.Any, CultureInfo.InvariantCulture, out double height)) {
                    person.Height = height;
                }

                if (double.TryParse(json.width, NumberStyles.Any, CultureInfo.InvariantCulture, out double width)) {
                    person.Width = width;
                }

                if (!string.IsNullOrWhiteSpace(json.scheme_url) && Uri.TryCreate(json.scheme_url, UriKind.Absolute, out Uri schemeUrl))
                    person.SchemeUrl = schemeUrl;
            }

            return person;
        }

        public override string ToString() {
            return $"{Id}: {FIO}";
        }

    }


}
