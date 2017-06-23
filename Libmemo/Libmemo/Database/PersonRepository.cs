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



        private async Task<long?> GetLastModified() => await Task.Run(async () => Settings.LastModified ??
            (await GetItems()).OrderByDescending(i => i.LastModified).FirstOrDefault()?.LastModified
        );






        #region Load
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

        //обработка полученных данных
        private async Task LoadDatabaseFromJson(JsonData result) {
            await DeletePersons(result.delete);

            await AddNewUsers(result.users);
            await AddNewPersons(result.persons);

            SaveLastModified(result);
        }

        private async Task DeletePersons(List<PersonJsonDelete> list) =>
            await DeleteItems(list.Select(i => i.id));

        private async Task UpdatePersons(List<PersonJsonUpdate> list) {
            var update = new List<Person>();

            foreach (var item in list) {
                var person = new Person();

                switch (item.type) {
                    case "l": person.PersonType = PersonType.Alive; break;
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
                    if ()
                        //TODO доделать
                }
            }
        }




        //сохранение в базу полученных пользователей
        private async Task AddNewPersons(List<PersonJsonAdd> list) => await SaveItems(
            list.Where(i => !string.IsNullOrWhiteSpace(i.first_name)
                && double.TryParse(i.latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat)
                && double.TryParse(i.longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
            .Select(i => new Person {
                Id = i.id,
                LastModified = i.modified,
                Owner = i.owner,
                FirstName = i.first_name.Trim(),
                SecondName = string.IsNullOrWhiteSpace(i.second_name) ? null : i.second_name.Trim(),
                LastName = string.IsNullOrWhiteSpace(i.last_name) ? null : i.last_name.Trim(),
                DateBirth = DateTime.TryParse(i.date_birth, out DateTime dBirth) ? (DateTime?)dBirth : null,
                DateDeath = DateTime.TryParse(i.date_death, out DateTime dDeath) ? (DateTime?)dDeath : null,
                Latitude = double.Parse(i.latitude, NumberStyles.Any, CultureInfo.InvariantCulture),
                Longitude = double.Parse(i.longitude, NumberStyles.Any, CultureInfo.InvariantCulture),
                Text = string.IsNullOrWhiteSpace(i.text) ? null : i.text.Trim(),
                Icon = string.IsNullOrWhiteSpace(i.icon) ? null : i.icon,
                ImageUrl = i.image_url,

                Height = double.TryParse(i.height, NumberStyles.Any, CultureInfo.InvariantCulture, out double height) ? (double?)height : null,
                Width = double.TryParse(i.width, NumberStyles.Any, CultureInfo.InvariantCulture, out double width) ? (double?)width : null,
                SchemeUrl = i.scheme_url
            })
        );

        //сохранение в базу полученных пользователей
        private async Task AddNewUsers(List<UserJsonAdd> list) => await SaveItems(
            list.Where(i => !string.IsNullOrWhiteSpace(i.first_name))
            .Select(i => new User {
                Id = i.id,
                LastModified = i.modified,
                Owner = i.owner,
                FirstName = i.first_name.Trim(),
                SecondName = string.IsNullOrWhiteSpace(i.second_name) ? null : i.second_name.Trim(),
                LastName = string.IsNullOrWhiteSpace(i.last_name) ? null : i.last_name.Trim(),
                DateBirth = DateTime.TryParse(i.date_birth, out DateTime dBirth) ? (DateTime?)dBirth : null,
                Icon = string.IsNullOrWhiteSpace(i.icon) ? null : i.icon,
                ImageUrl = string.IsNullOrWhiteSpace(i.image_url) ? null : i.image_url
            })
        );

        //удаляет устаревшие элементы


        //сохраняет последнюю дату синхронизации с сервером
        private void SaveLastModified(JsonData data) {
            var modified = data.users.Select(i => i.modified)
                .Concat(data.persons.Select(i => i.modified))
                .Concat(data.delete.Select(i => i.modified))
                .DefaultIfEmpty(0)
                .Max();
                
            if (modified != default(long)) Settings.LastModified = modified;
        }

        #endregion





    }
}
