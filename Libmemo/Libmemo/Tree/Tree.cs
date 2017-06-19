using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo {
    class Tree {
        private readonly User _user;
        public User User { get => _user; }

        private readonly TreeItem _root;
        public TreeItem Root { get => _root; }



        public Tree(User user) {
            _user = user;
            _root = new TreeItem(user);

        }

        public Task Calculate() => Task.Run(async () => {
            var persons = await App.Database.GetItems<Person>();
            var users = await App.Database.GetItems<User>();

            var q = 1;
        });




    }

    class TreeItem {
        public TreeItem(IDatabaseSavable person) {
            Current = person;
        }

        public IDatabaseSavable Current { get; private set; }
        public TreeItemView Mother { get; set; }
        public TreeItemView Father { get; set; }
        public List<TreeItemView> Siblings { get; set; }
    }
}
