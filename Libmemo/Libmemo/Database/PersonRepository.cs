using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;

using SQLite;

using Xamarin.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace Libmemo {
    public class PersonRepository {

        private const int DATA_LOAD_TIMEOUT = 30;
        private SQLiteConnection database;

        public PersonRepository() {
            string databasePath = DependencyService.Get<ISQLite>().GetDatabasePath("database.db");
            database = new SQLiteConnection(databasePath);         
            database.CreateTable<Person.PersonDB>();
        }

        public event EventHandler Updated;

        #region CRUD

        public async Task<Person> GetById(int id) => await Task.Run(() => {
            lock (database) {
                var item = database.Table<Person.PersonDB>().SingleOrDefault(i => i._Id == id);
                return Person.ConvertFromDatabase(item);
            }
        });

        public async Task<List<Person>> GetList() => await Task.Run(() => {
            lock (database) {
                var list = new List<Person>();
                foreach (var item in database.Table<Person.PersonDB>()) 
                    list.Add(Person.ConvertFromDatabase(item));

                return list;
            }
        });
        public async Task<List<Person>> GetList(PersonType type) => await Task.Run(() => {
            lock (database) {
                var list = new List<Person>();
                foreach (var item in database.Table<Person.PersonDB>()) {
                    if (item._PersonType != type) continue;
                    list.Add(Person.ConvertFromDatabase(item));
                }
                return list;
            }
        });
        public async Task<List<Person>> GetList(PersonType[] types) => await Task.Run(() => {
            var set = new HashSet<PersonType>();
            foreach (var type in types) set.Add(type);

            lock (database) {
                var list = new List<Person>();
                foreach (var item in database.Table<Person.PersonDB>()) {
                    if (!set.Contains(item._PersonType)) continue;
                    list.Add(Person.ConvertFromDatabase(item));
                }
                return list;
            }
        });

        public async Task<Dictionary<int, Person>> GetDictionary() =>
            (await GetList()).ToDictionary(i => i.Id);
        public async Task<Dictionary<int, Person>> GetDictionary(PersonType type) =>
            (await GetList(type)).ToDictionary(i => i.Id);
        public async Task<Dictionary<int, Person>> GetDictionary(PersonType[] types) =>
            (await GetList(types)).ToDictionary(i => i.Id);


        private async Task SaveItem(Person item) => await Task.Run(() => {
            var save = Person.ConvertToDatabase(item);
            if (database.Update(save) == 0) database.Insert(save);
        });

        private async Task SaveItems(IEnumerable<Person> items) => await Task.Run(() => {
            lock (database) {
                foreach (var item in items) {
                    var save = Person.ConvertToDatabase(item);
                    if (database.Update(save) == 0) database.Insert(save);
                }
            }              
        });

        private async Task DeleteItems(IEnumerable<int> ids) => await Task.Run(() => {
            lock (database) {
                foreach (var id in ids)
                    database.Delete<Person.PersonDB>(id);
            }
        });

        private async Task DeleteAllItems() => await Task.Run(() => {
            lock (database) {
                database.DeleteAll<Person.PersonDB>();
            }    
        });

        #endregion

        #region Load

        private async Task<long?> GetLastModified() => await Task.Run(async () => Settings.LastModified ??
            (await GetList()).OrderByDescending(i => i.LastModified).FirstOrDefault()?.LastModified
        );

        public async void Load(bool full = false) {
            long? lastModified = full ? null : await GetLastModified();

            var result = await SendRequest(lastModified);
            if (result == null) {
                return;
            }

            if (full) await DeleteAllItems();
            await LoadDatabaseFromJson(result);

            Updated?.Invoke(this, null);
        }

        private async Task<PersonJson> SendRequest(long? modified = null) {
            HttpClient client = new HttpClient {
                Timeout = TimeSpan.FromSeconds(DATA_LOAD_TIMEOUT)
            };

            var builder = new UriBuilder(Settings.DATABASE_URL);
            if (modified.HasValue)
                builder.Query = $"from={modified.Value.ToString()}";
            var uri = builder.ToString();

            try {
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PersonJson>(content);

                return result;
            } catch {
                return null;
            }
        }

        private async Task LoadDatabaseFromJson(PersonJson result) {
            var pList = GetPersonsList(result.update);
            var dList = GetDeleteList(result.delete);

            var deleteIdList = pList.Select(i => i.Id).Concat(dList.Select(i => i.Item1));
            await DeleteItems(deleteIdList);

            await SaveItems(pList);

            var lastModified = pList.Select(i => new long?(i.LastModified)).Concat(dList.Select(i => new long?(i.Item2))).OrderByDescending(i => i.Value).FirstOrDefault();
            if (lastModified.HasValue) Settings.LastModified = lastModified;
        }

        private IEnumerable<(int, long)> GetDeleteList(IEnumerable<PersonJson.Delete> list) =>
            list.Where(i => i.id.HasValue && i.modified.HasValue).Select(i => (i.id.Value, i.modified.Value));

        private IEnumerable<Person> GetPersonsList(List<PersonJson.Update> list) {
            var update = new List<Person>();

            foreach (var item in list) {
                var person = Person.ConvertFromJson(item);
                if (person == null) continue;
                update.Add(person);
            }

            return update;
        }
        
        #endregion

        public async Task AddPerson(Person person) {
            await SaveItem(person);
            Updated?.Invoke(this, null);
        }

        public async Task DeletePerson(int id) {
            await DeleteItems(new int[] { id });
            Updated?.Invoke(this, null);
        }
    }
}
