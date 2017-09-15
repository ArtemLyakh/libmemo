using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo.Json
{
    public class Auth
    {
        public int id { get; set; }
        public bool is_admin { get; set; }
        public string fio { get; set; }
        public string email { get; set; }
    }

    public class Message
    {
        public string message { get; set; }
    }

    public class User
    {
        public int id { get; set; }
        public string fio { get; set; }
        public string email { get; set; }
    }

    public class AccountData
    {
        public string first_name { get; set; }
        public string second_name { get; set; }
        public string last_name { get; set; }
        public string date_birth { get; set; }
        public string photo_url { get; set; }
    }

    public class AdminPersonalData
    {
        public int id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string second_name { get; set; }
        public string last_name { get; set; }
        public string date_birth { get; set; }
        public string photo_url { get; set; }
    }

    class PersonDelete
    {
        public int id { get; set; }
    }






    public class UserListEntry
    {
        public int id { get; set; }
        public string type { get; set; }
        public string first_name { get; set; }
        public string second_name { get; set; }
        public string last_name { get; set; }
        public string preview_image_url { get; set; }
    }
    public class UserList
    {
        public List<UserListEntry> relatives { get; set; }
    }






    public class Tree
    {
        public class Structure
        {
            public int current { get; set; }
            public int? mother { get; set; }
            public int? father { get; set; }
            public List<int> siblings { get; set; }
        }

        public int user { get; set; }
        public Dictionary<int, UserListEntry> persons { get; set; }
        public Dictionary<int, Structure> structure { get; set; }
    }
    public class TreeSave
    {
        public int person { get; set; }
        public int? mother { get; set; }
        public int? father { get; set; }
        public List<int> siblings { get; set; }
    }



    public class PersonDetail
    {
        public class Tree
        {
            public int id { get; set; }
            public string fio { get; set; }
        }

        public List<Tree> trees { get; set; }
        public Json.Person person { get; set; }
    }



	public class Person
	{
		public string type { get; set; }
		public int? id { get; set; }
		public int? owner { get; set; }

		public string first_name { get; set; }
		public string second_name { get; set; }
		public string last_name { get; set; }
		public string date_birth { get; set; }

		public string icon_url { get; set; }
		public string photo_url { get; set; }
		public string preview_image_url { get; set; }
        public Dictionary<int, string> photos { get; set; }
 
		public string latitude { get; set; }
		public string longitude { get; set; }
		public string date_death { get; set; }
		public string text { get; set; }
		public string height { get; set; }
		public string width { get; set; }
		public string scheme_url { get; set; }

		public string address { get; set; }
		public string city { get; set; }

		public string section { get; set; }
		public string grave_number { get; set; }
	}

	public class Error
	{
		public string error { get; set; }
	}
}

namespace Libmemo.Json.Admin
{
    public class User
    {
        public int id { get; set; }
        public string fio { get; set; }
        public string email { get; set; }
    }

    public class AccountData : Json.AccountData
    {
        public string email { get; set; }
    }

    public class Realtive
    {
        public Json.Person person { get; set; }
        public User owner { get; set; }
    }

}