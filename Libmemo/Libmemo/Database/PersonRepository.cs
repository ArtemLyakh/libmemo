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

            this.LoadSuccess += () => App.ToastNotificator.Show("База данных загружена");
            this.LoadFail += () => App.ToastNotificator.Show("Ошибка загрузки данных с сервера");
        }

        public event Action LoadSuccess;
        public event Action LoadFail;

        #region CRUD

        public async Task<Person> GetById(int id) => await Task.Run(() => {
            lock (database) {
                var item = database.Table<Person.PersonDB>().SingleOrDefault(i => i._Id == id);
                return Person.ConvertFromDatabase(item);
            }
        });

        public async Task<List<Person>> GetItems() => await Task.Run(() => {
            lock (database) {
                var list = new List<Person>();
                foreach (var item in database.Table<Person.PersonDB>()) {
                    var person = Person.ConvertFromDatabase(item);
                    list.Add(person);
                }
                return list;
            }
        });
        public async Task<List<Person>> GetItems(PersonType type) => await Task.Run(() => {
            lock (database) {
                var list = new List<Person>();
                foreach (var item in database.Table<Person.PersonDB>()) {
                    if (item._PersonType != type) continue;
                    var person = Person.ConvertFromDatabase(item);
                    list.Add(person);
                }
                return list;
            }
        });

        public async Task SaveItem(Person item) => await Task.Run(() => {
            var save = Person.ConvertToDatabase(item);
            if (database.Update(save) == 0) database.Insert(save);
        });

        public async Task SaveItems(IEnumerable<Person> items) => await Task.Run(() => {
            lock (database) {
                foreach (var item in items) {
                    var save = Person.ConvertToDatabase(item);
                    if (database.Update(save) == 0) database.Insert(save);
                }
            }              
        });

        public async Task DeleteItems(IEnumerable<int> ids) => await Task.Run(() => {
            lock (database) {
                foreach (var id in ids)
                    database.Delete<Person.PersonDB>(id);
            }
        });

        public async Task DeleteAllItems() => await Task.Run(() => {
            lock (database) {
                database.DeleteAll<Person.PersonDB>();
            }    
        });

        #endregion

        #region Load

        private async Task<long?> GetLastModified() => await Task.Run(async () => Settings.LastModified ??
            (await GetItems()).OrderByDescending(i => i.LastModified).FirstOrDefault()?.LastModified
        );

        public async void Load(bool full = false) {
            long? lastModified = full ? null : await GetLastModified();

            var result = await SendRequest(lastModified);
            if (result == null) {
                LoadFail?.Invoke();
                return;
            }

            if (full) await DeleteAllItems();
            await LoadDatabaseFromJson(result);

            LoadSuccess?.Invoke();
        }

        private async Task<JsonData> SendRequest(long? modified = null) {
            HttpClient client = new HttpClient {
                Timeout = TimeSpan.FromSeconds(DATA_LOAD_TIMEOUT)
            };

            var builder = new UriBuilder(Settings.DATABASE_URL);
            if (modified.HasValue)
                builder.Query = $"from={modified.Value.ToString()}";
            var uri = builder.ToString();

            try {
                var responce = await client.GetAsync(uri);
                responce.EnsureSuccessStatusCode();

                var content = await responce.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<JsonData>(content);

                return result;
            } catch {
                return null;
            }
        }

        private async Task LoadDatabaseFromJson(JsonData result) {
            var pList = GetPersonsList(result.update);
            var dList = GetDeleteList(result.delete);

            var deleteIdList = pList.Select(i => i.Id).Concat(dList.Select(i => i.Item1));
            await DeleteItems(deleteIdList);

            await SaveItems(pList);

            var lastModified = pList.Select(i => new long?(i.LastModified)).Concat(dList.Select(i => new long?(i.Item2))).OrderByDescending(i => i.Value).FirstOrDefault();
            if (lastModified.HasValue) Settings.LastModified = lastModified;
        }

        private IEnumerable<Tuple<int, long>> GetDeleteList(IEnumerable<JsonData.PersonJsonDelete> list) =>
            list.Where(i => i.id.HasValue && i.modified.HasValue).Select(i => Tuple.Create(i.id.Value, i.modified.Value));

        private IEnumerable<Person> GetPersonsList(List<JsonData.PersonJsonUpdate> list) {
            var update = new List<Person>();

            foreach (var item in list) {
                var person = Person.ConvertFromJson(item);
                if (person == null) continue;
                update.Add(person);
            }

            return update;
        }
        
        #endregion
    }
}
