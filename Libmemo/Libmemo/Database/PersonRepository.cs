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
        private SQLiteConnection database;

        #region Constructor
        public PersonRepository() {
            string databasePath = DependencyService.Get<ISQLite>().GetDatabasePath(Settings.DatabaseName);
            database = new SQLiteConnection(databasePath);
            database.CreateTable<Person>();

            this.LoadSuccess += () => {
                App.ToastNotificator.Show("Получены данные с сервера");
            };
            this.LoadFail += () => {
                App.ToastNotificator.Show("Ошибка загрузки данных с сервера");
            };
        }
        #endregion

        #region Load from server

        #region Events
        public event Action LoadSuccess;
        public event Action LoadFail;
        #endregion

        #region Load
        public async void Load() {
            long? lastModified = await GetLastModified();

            var result = await SendRequest(lastModified);
            if (result == null) {
                LoadFail?.Invoke();
                return;
            }

            await AddNewPersons(result.add);
            await DeleteNewPersons(result.delete);
            SaveLastModified(result);

            LoadSuccess?.Invoke();
        }
        #endregion

        #region Helpers
        private async Task<long?> GetLastModified() {
            return await Task.Factory.StartNew<long?>(() => {
                return Settings.LastModified
                    ?? database.Table<Person>().OrderByDescending(o => o.LastModified).FirstOrDefault()?.LastModified;
            });
        }
        private void test() {
            var q = GetLastModified();
            q.Start();

        }
        private async Task<JsonData> SendRequest(long? modified = null) {
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(10 * 1000 * 1000 * 10);
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
        private async Task AddNewPersons(List<PersonJsonAdd> list) {
            List<Person> arrAdd = new List<Person>();

            foreach (var item in list) {
                if (!Double.TryParse(item.lat, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat)
                    || !Double.TryParse(item.lon, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon)
                    || String.IsNullOrWhiteSpace(item.fio)
                    )
                    continue;

                Person entry = new Person() {
                    Id = item.id,
                    LastModified = item.modified,
                    Name = item.fio,
                    Link = String.IsNullOrWhiteSpace(item.link) ? null : item.link,
                    DateBirth = DateTime.TryParse(item.date_birth, out DateTime dBirth) ? (DateTime?)dBirth : null,
                    DateDeath = DateTime.TryParse(item.date_death, out DateTime dDeath) ? (DateTime?)dDeath : null,
                    Latitude = lat,
                    Longitude = lon,
                    Text = String.IsNullOrWhiteSpace(item.text) ? null : item.text,
                    Image = String.IsNullOrWhiteSpace(item.image) ? null : item.image
                };
                arrAdd.Add(entry);
            }

            if (arrAdd.Count > 0)
                await SaveItems(arrAdd);
        }
        private async Task DeleteNewPersons(List<PersonJsonDelete> list) {
            List<int> arrDelete = (from i in list select i.id).ToList();

            if (arrDelete.Count > 0)
                await DeleteItems(arrDelete);
        }
        private void SaveLastModified(JsonData data) {
            long add = 0,
                del = 0;

            if (data.add.Count > 0) {
                add = data.add.Max(i => i.modified);
            }
            if (data.delete.Count > 0) {
                del = data.delete.Max(i => i.modified);
            }

            long res = Math.Max(add, del);
            if (res != 0) {
                Settings.LastModified = res;
            }
        }
        #endregion

        #endregion

        #region Database CRUD
        public async Task<Person> GetById(int id) {
            return await Task.Factory.StartNew<Person>(() => {
                lock (database) {
                    return (from i in database.Table<Person>() where i.Id == id select i).FirstOrDefault();
                }
            });
        }

        public async Task<IEnumerable<Person>> GetItems() {
            return await Task.Factory.StartNew<IEnumerable<Person>>(() => {
                lock (database) {
                    return (from i in database.Table<Person>() select i)?.ToList();
                }
            });
        }
        public async Task<IEnumerable<Person>> GetItems(string nameFilter) {
            return await Task.Factory.StartNew(() => _GetItems(nameFilter));
        }
        private IEnumerable<Person> _GetItems(string nameFilter) {
            nameFilter = nameFilter.ToLower();
            lock (database) {
                foreach (var item in database.Table<Person>()) {
                    if (item.Name.IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                        yield return item;
                }
            }
        }


        public async Task SaveItems(IEnumerable<Person> items) {
            await Task.Factory.StartNew(() => {
                lock (database) {
                    foreach (var item in items) {
                        if (database.Update(item) == 0) {
                            database.Insert(item);
                        }
                    }
                }
            });
        }

        public async Task DeleteItems(IEnumerable<int> ids) {
            await Task.Factory.StartNew(() => {
                lock (database) {
                    foreach (var id in ids) {
                        var q = database.Delete<Person>(id);
                    }
                }
            });
        }

        public async Task DeleteAllItems() {
            await Task.Factory.StartNew(() => {
                lock (database) {
                    database.DeleteAll<Person>();
                }
            });
        }
        #endregion

        public string GetBase64ProfilePictureById(int id) {
            lock (database) {
                return (from i in database.Table<Person>() where i.Id == id select i).FirstOrDefault()?.Image;
            }
        }
    }
}
