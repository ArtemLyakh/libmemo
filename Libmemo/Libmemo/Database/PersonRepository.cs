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
            database.CreateTable<Person>();

            this.LoadSuccess += () => App.ToastNotificator.Show("База данных загружена");
            this.LoadFail += () => App.ToastNotificator.Show("Ошибка загрузки данных с сервера");
        }

        public event Action LoadSuccess;
        public event Action LoadFail;

        #region CRUD

        public async Task<Person> GetById(int id) => await Task.Run(() => {
            lock (database) return database.Table<Person>().SingleOrDefault(i => i.Id == id);
        });

        public async Task<List<Person>> GetItems() => await Task.Run(() => {
            lock (database) return database.Table<Person>().ToList();
        });
        public async Task<List<Person>> GetItems(PersonType type) => await Task.Run(() => {
            lock (database) return database.Table<Person>().Where(i => i.PersonType == type).ToList();
        });

        public async Task SaveItems(IEnumerable<Person> items) => await Task.Run(() => {
            lock (database)
                foreach (var item in items)
                    if (database.Update(item) == 0) database.Insert(item);
        });

        public async Task DeleteItems(IEnumerable<int> ids) => await Task.Run(() => {
            lock (database)
                foreach (var id in ids) database.Delete<Person>(id);
        });

        public async Task DeleteAllItems() => await Task.Run(() => {
            lock (database) database.DeleteAll<Person>();
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
                var result = JsonConvert.DeserializeObject<JsonData>(content, new IsoDateTimeConverter { DateTimeFormat = "dd.MM.yyyy" });

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

        private IEnumerable<Tuple<int, long>> GetDeleteList(IEnumerable<PersonJsonDelete> list) =>
            list.Where(i => i.id.HasValue && i.modified.HasValue).Select(i => Tuple.Create(i.id.Value, i.modified.Value));

        private IEnumerable<Person> GetPersonsList(List<PersonJsonUpdate> list) {
            var update = new List<Person>();

            foreach (var item in list) {
                var person = new Person();

                switch (item.type) {
                    case "a": person.PersonType = PersonType.Alive; break;
                    case "d": person.PersonType = PersonType.Dead; break;
                    default: continue;
                }

                if (item.id.HasValue) person.Id = item.id.Value;
                else continue;

                if (item.modified.HasValue) person.LastModified = item.modified.Value;
                else continue;

                if (item.owner.HasValue) person.Owner = item.owner.Value;
                else continue;


                if (!string.IsNullOrWhiteSpace(item.first_name)) person.FirstName = item.first_name;
                else continue;

                if (!string.IsNullOrWhiteSpace(item.second_name)) person.SecondName = item.second_name;

                if (!string.IsNullOrWhiteSpace(item.last_name)) person.LastName = item.last_name;

                if (DateTime.TryParse(item.date_birth, out DateTime dBirth)) person.DateBirth = dBirth;

                if (!string.IsNullOrWhiteSpace(item.icon)) person.Icon = item.icon;

                if (!string.IsNullOrWhiteSpace(item.photo_url) && Uri.TryCreate(item.photo_url, UriKind.Absolute, out Uri photoUrl))
                    person.ImageUrl = photoUrl;


                if (person.PersonType == PersonType.Dead) {
                    if (double.TryParse(item.latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double latitude)) {
                        person.Latitude = latitude;
                    } else continue;

                    if (double.TryParse(item.longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double longitude)) {
                        person.Longitude = longitude;
                    } else continue;

                    if (DateTime.TryParse(item.date_death, out DateTime dDeath)) person.DateDeath = dDeath;

                    if (!string.IsNullOrWhiteSpace(item.text)) person.Text = item.text;

                    if (double.TryParse(item.height, NumberStyles.Any, CultureInfo.InvariantCulture, out double height)) {
                        person.Height = height;
                    }

                    if (double.TryParse(item.width, NumberStyles.Any, CultureInfo.InvariantCulture, out double width)) {
                        person.Width = width;
                    }

                    if (!string.IsNullOrWhiteSpace(item.scheme_url) && Uri.TryCreate(item.scheme_url, UriKind.Absolute, out Uri schemeUrl))
                        person.SchemeUrl = schemeUrl;
                }

                update.Add(person);
            }

            return update;
        }
        
        #endregion
    }
}
