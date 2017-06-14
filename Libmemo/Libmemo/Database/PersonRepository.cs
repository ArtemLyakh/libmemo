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

        private const int DATA_LOAD_TIMEOUT = 20;
        private SQLiteConnection database;

        public PersonRepository() {
            string databasePath = DependencyService.Get<ISQLite>().GetDatabasePath("database.db");
            database = new SQLiteConnection(databasePath);
            database.CreateTable<Person>();
            database.CreateTable<User>();

            this.LoadSuccess += () => {
                App.ToastNotificator.Show("База данных загружена");
            };
            this.LoadFail += () => {
                App.ToastNotificator.Show("Ошибка загрузки данных с сервера");
            };
        }

        public event Action LoadSuccess;
        public event Action LoadFail;

        #region Load
        public async void Load(bool full = false) {
            long? lastModified = full ? null : await GetLastModified();

            var result = await SendRequest(lastModified);
            if (result == null) {
                LoadFail?.Invoke();
                return;
            }

            if (full) {
                await DeleteAllItems<Person>();
                await DeleteAllItems<User>();
            }
            await LoadDatabaseFromJson(result);

            LoadSuccess?.Invoke();
        }

        private async Task<JsonData> SendRequest(long? modified = null) {
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, DATA_LOAD_TIMEOUT);
            string request = Settings.DataUrl;
            if (modified != null) request += "?from=" + modified.ToString();
            try {
                var responce = await client.GetAsync(request);
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
            await AddNewUsers(result.users);
            await AddNewPersons(result.persons);
            await DeletePersonsAndUsers(result.delete);

            SaveLastModified(result);
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
        private async Task DeletePersonsAndUsers(List<PersonJsonDelete> list) {
            await DeleteItems<Person>(list.Select(i => i.id));
            await DeleteItems<User>(list.Select(i => i.id));
        }

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


        //выбирает объект из указанной таблицы по id
        public async Task<T> GetById<T>(int id) where T : IDatabaseSavable, new() => await Task.Run(() => {
            lock(database) return database.Table<T>().SingleOrDefault(i => i.Id == id);
        });

        //выбирает все объекты из указанной таблицы
        public async Task<IEnumerable<T>> GetItems<T>() where T : IDatabaseSavable, new() => await Task.Run(() => {
            lock (database) return database.Table<T>().ToList();
        });

        //сохраняет/обновляет данные
        public async Task SaveItems<T>(IEnumerable<T> items) where T : IDatabaseSavable, new() => await Task.Run(() => {
            lock (database) 
                foreach (var item in items) if (database.Update(item) == 0) database.Insert(item);
        });

        //удаляет данные
        public async Task DeleteItems<T>(IEnumerable<int> ids) where T : IDatabaseSavable, new() => await Task.Run(() => {
            lock (database)
                foreach (var id in ids) database.Delete<T>(id);
        });

        //удаляет все данные
        public async Task DeleteAllItems<T>() where T : IDatabaseSavable, new() => await Task.Run(() => {
            lock (database)
                database.DeleteAll<T>();
        });



        private async Task<long?> GetLastModified() => await Task.Run(() => {
            var last = Settings.LastModified ?? database.Table<Person>().Select(i => i.LastModified)
                .Union(database.Table<User>().Select(i => i.LastModified))
                .OrderByDescending(i => i)
                .FirstOrDefault();
            return last == default(long) ? null : (long?)last;
        });

    }
}
