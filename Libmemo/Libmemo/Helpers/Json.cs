using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo.Json {
    public class Auth {
        public int id { get; set; }
        public bool is_admin { get; set; }
        public string fio { get; set; }
        public string email { get; set; }
    }

    public class Message {
        public string message { get; set; }
    }

    public class Register {
        public Auth auth { get; set; }
        public PersonJson.Update person { get; set; }
    }

    public class User {
        public int id { get; set; }
        public string fio { get; set; }
        public string email { get; set; }
    }

    public class AccountData {
        public string first_name { get; set; }
        public string second_name { get; set; }
        public string last_name { get; set; }
        public string date_birth { get; set; }
        public string photo_url { get; set; }
    }

    public class AdminPersonalData {
        public int id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string second_name { get; set; }
        public string last_name { get; set; }
        public string date_birth { get; set; }
        public string photo_url { get; set; }
    }

    class PersonDelete {
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
            int id { get; set; }
            string fio { get; set; }
        }

        public List<Tree> trees { get; set; }
        public Json.Person person { get; set; }
    }
}
