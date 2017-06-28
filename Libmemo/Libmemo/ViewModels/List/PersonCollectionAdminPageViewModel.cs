using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Libmemo {
    public class PersonCollectionAdminPageViewModel : BaseListViewModel<PersonCollectionAdminPageViewModel.Person> {
        public class Person : ISearchFiltrable {
            public string FilterString => Fio;

            public ImageSource Image { get; set; }
            public bool IsDead { get; set; }
            public int Id { get; set; }
            public string Fio { get; set; }
        }

        public PersonCollectionAdminPageViewModel() : base() {
            Task.Run(async () => await Load());
        }



        private async Task Load() {
            if (!AuthHelper.IsAdmin) {
                await AuthHelper.ReloginAsync();
                return;
            }

            this.Data = (await App.Database.GetItems())
                .Select(i => new Person {
                    Id = i.Id,
                    Fio = i.FIO,
                    IsDead = i.PersonType == PersonType.Dead,
                    Image = string.IsNullOrWhiteSpace(i.Icon)
                                ? null :
                                ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(i.Icon)))
                });

        }
    }
}
