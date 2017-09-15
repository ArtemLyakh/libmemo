using System;
using System.Collections.Generic;
using System.Globalization;

namespace Libmemo.Models
{
    public abstract class Person
    {
        public int Id { get; set; }
        public int Owner { get; set; }

        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string LastName { get; set; }
        public string FIO => string.Join(" ", new string[] { LastName, FirstName, SecondName }).Trim();

        public DateTime? DateBirth { get; set; }

        public Uri Icon { get; set; }
        public Uri Image{ get; set; }
        public Uri PreviewImage { get; set; }

        public Dictionary<int, Uri> Images { get; set; } = new Dictionary<int, Uri>();

        public Person(int id, int owner, string firstName)
        {
            Id = id;
            Owner = owner;
            FirstName = firstName;
        }


        public static Person Convert(Json.Person json)
        {
            switch (json.type) {
                case "alive": return AlivePerson.ConvertFromJson(json);
                case "dead": return DeadPerson.ConvertFromJson(json);
                case "user": return UserPerson.ConvertFromJson(json);
                default: throw new ArgumentException($"Type [{json.type}] is invalid");
            }
        }
    }

    public class DeadPerson : Person {
		public double Latitude { get; set; }
		public double Longitude { get; set; }

		public DateTime? DateDeath { get; set; }
		
		public string Text { get; set; }

		public double? Height { get; set; }
		public double? Width { get; set; }
		public Uri Scheme { get; set; }

        public string Section { get; set; }
        public double? GraveNumber { get; set; }

        public string Address { get; set; }
        public string City { get; set; }

        public DeadPerson(int id, int owner, string firstName, double latitude, double longitude) : base(id, owner, firstName)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public static DeadPerson ConvertFromJson(Json.Person json)
        {
            if (!json.id.HasValue)
                throw new ArgumentException("Id is not set");
            if (!json.owner.HasValue)
                throw new ArgumentException("Owner is not set");
            if (string.IsNullOrWhiteSpace(json.first_name))
                throw new ArgumentException("FirstName is not set");
            if (!double.TryParse(json.latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double latitude))
                throw new ArgumentException("Latitude is invalid");
            if (!double.TryParse(json.longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double longitude))
                throw new ArgumentException("Longitude is invalid");



            var person = new DeadPerson(
                id: json.id.Value,
                owner: json.owner.Value,
                firstName: json.first_name,
                latitude: latitude,
                longitude: longitude
            );



            if (!string.IsNullOrWhiteSpace(json.second_name))
                person.SecondName = json.second_name;
            if (!string.IsNullOrWhiteSpace(json.last_name))
                person.LastName = json.last_name;

            if (DateTime.TryParse(json.date_birth, out DateTime dateBirth))
                person.DateBirth = dateBirth;
            if (DateTime.TryParse(json.date_death, out DateTime dateDeath))
                person.DateDeath = dateDeath;

            if (!string.IsNullOrWhiteSpace(json.icon_url) && Uri.TryCreate(json.icon_url, UriKind.Absolute, out Uri icon))
                person.Icon = icon;
            if (!string.IsNullOrWhiteSpace(json.photo_url) && Uri.TryCreate(json.photo_url, UriKind.Absolute, out Uri image))
                person.Image = image;
            if (!string.IsNullOrWhiteSpace(json.preview_image_url) && Uri.TryCreate(json.preview_image_url, UriKind.Absolute, out Uri previewImage))
                person.PreviewImage = previewImage;
            if (json.photos != null && json.photos.Count > 0)
			{
				foreach (var img in json.photos)
				{
                    if (!string.IsNullOrWhiteSpace(img.Value) && Uri.TryCreate(img.Value, UriKind.Absolute, out Uri imgUri))
                        person.Images[img.Key] = imgUri;
				}
			}

            if (!string.IsNullOrWhiteSpace(json.text))
                person.Text = json.text;

            if (double.TryParse(json.height, NumberStyles.Any, CultureInfo.InvariantCulture, out double height)) 
                person.Height = height;
            if (double.TryParse(json.width, NumberStyles.Any, CultureInfo.InvariantCulture, out double width)) 
                person.Width = width;

            if (!string.IsNullOrWhiteSpace(json.scheme_url) && Uri.TryCreate(json.scheme_url, UriKind.Absolute, out Uri scheme))
                person.Scheme = scheme;

            if (!string.IsNullOrWhiteSpace(json.section))
                person.Section = json.section;
            if (double.TryParse(json.grave_number, NumberStyles.Any, CultureInfo.InvariantCulture, out double graveNumber))
                person.GraveNumber = graveNumber;

            if (!string.IsNullOrWhiteSpace(json.address))
                person.Address = json.address;
            if (!string.IsNullOrWhiteSpace(json.city))
                person.City = json.city;


            return person;
        }
    }

    public class AlivePerson : Person 
    {
        public AlivePerson(int id, int owner, string firstName) : base(id, owner, firstName) { }

        public static AlivePerson ConvertFromJson(Json.Person json)
        {
            if (!json.id.HasValue)
                throw new ArgumentException("Id is not set");
            if (!json.owner.HasValue)
                throw new ArgumentException("Owner is not set");
            if (string.IsNullOrWhiteSpace(json.first_name))
                throw new ArgumentException("FirstName is not set");



            var person = new AlivePerson(
                id: json.id.Value,
                owner: json.owner.Value,
                firstName: json.first_name
            );



            if (!string.IsNullOrWhiteSpace(json.second_name))
                person.SecondName = json.second_name;
            if (!string.IsNullOrWhiteSpace(json.last_name))
                person.LastName = json.last_name;

            if (DateTime.TryParse(json.date_birth, out DateTime dateBirth))
                person.DateBirth = dateBirth;

            if (!string.IsNullOrWhiteSpace(json.icon_url) && Uri.TryCreate(json.icon_url, UriKind.Absolute, out Uri icon))
                person.Icon = icon;
            if (!string.IsNullOrWhiteSpace(json.photo_url) && Uri.TryCreate(json.photo_url, UriKind.Absolute, out Uri image))
                person.Image = image;
            if (!string.IsNullOrWhiteSpace(json.preview_image_url) && Uri.TryCreate(json.preview_image_url, UriKind.Absolute, out Uri previewImage))
                person.PreviewImage = previewImage;
			if (json.photos.Count > 0)
			{
				foreach (var img in json.photos)
				{
					if (!string.IsNullOrWhiteSpace(img.Value) && Uri.TryCreate(img.Value, UriKind.Absolute, out Uri imgUri))
						person.Images[img.Key] = imgUri;
				}
			}


            return person;
        }
    }

	public class UserPerson : Person
	{
		public UserPerson(int id, int owner, string firstName) : base(id, owner, firstName) { }

        public static UserPerson ConvertFromJson(Json.Person json)
        {
            if (!json.id.HasValue)
                throw new ArgumentException("Id is not set");
            if (!json.owner.HasValue)
                throw new ArgumentException("Owner is not set");
            if (string.IsNullOrWhiteSpace(json.first_name))
                throw new ArgumentException("FirstName is not set");



            var person = new UserPerson(
                id: json.id.Value,
                owner: json.owner.Value,
                firstName: json.first_name
            );



            if (!string.IsNullOrWhiteSpace(json.second_name))
                person.SecondName = json.second_name;
            if (!string.IsNullOrWhiteSpace(json.last_name))
                person.LastName = json.last_name;

            if (DateTime.TryParse(json.date_birth, out DateTime dateBirth))
                person.DateBirth = dateBirth;

            if (!string.IsNullOrWhiteSpace(json.icon_url) && Uri.TryCreate(json.icon_url, UriKind.Absolute, out Uri icon))
                person.Icon = icon;
            if (!string.IsNullOrWhiteSpace(json.photo_url) && Uri.TryCreate(json.photo_url, UriKind.Absolute, out Uri image))
                person.Image = image;
            if (!string.IsNullOrWhiteSpace(json.preview_image_url) && Uri.TryCreate(json.preview_image_url, UriKind.Absolute, out Uri previewImage))
                person.PreviewImage = previewImage;
			if (json.photos.Count > 0)
			{
				foreach (var img in json.photos)
				{
					if (!string.IsNullOrWhiteSpace(img.Value) && Uri.TryCreate(img.Value, UriKind.Absolute, out Uri imgUri))
						person.Images[img.Key] = imgUri;
				}
			}


            return person;
        }
    }

}
