using System;
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

		public Uri IconUrl { get; set; }
		public Uri ImageUrl { get; set; }
		public Uri SmallImageUrl { get; set; }

        public Person(int id, int owner, string firstName)
        {
            Id = id;
            Owner = owner;
            FirstName = firstName;
        }
    }

    public class DeadPerson : Person {
		public double Latitude { get; set; }
		public double Longitude { get; set; }

		public DateTime? DateDeath { get; set; }
		
		public string Text { get; set; }

		public double? Height { get; set; }
		public double? Width { get; set; }
		public Uri SchemeUrl { get; set; }

        public DeadPerson(int id, int owner, string firstName, double latitude, double longitude) : base(id, owner, firstName)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    public class AlivePerson : Person 
    {
        public AlivePerson(int id, int owner, string firstName) : base(id, owner, firstName) { }
    }

	public class UserPerson : Person
	{
		public AlivePerson(int id, int owner, string firstName) : base(id, owner, firstName) { }
	}

}
