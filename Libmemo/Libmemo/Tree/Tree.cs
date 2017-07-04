using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Libmemo {
    class Tree {
        private readonly int _userId;
        public int UserId => _userId;

        public class Item {
            public Person Person { get; set; }
            public Item Mother { get; set; }
            public Item Father { get; set; }
            public List<Person> Siblings { get; set; } = new List<Person>();
        }
        public Item Root { get; set; } = null;


        public Tree (int userId) {
            this._userId = userId;
        }


        public class Json {
            public int? user { get; set; }
            public List<Row> data { get; set; }

            public class Row {
                public int? person { get; set; }
                public int? mother { get; set; }
                public int? father { get; set; }
                public List<int?> siblings { get; set; }
            }
        }
        public async Task<bool> LoadData() {
            try {
                using (var handler = new HttpClientHandler { CookieContainer = AuthHelper.CookieContainer })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) })
                using (var responce = await client.GetAsync(Settings.TREE_DATA_URL)) {
                    var str = await responce.Content.ReadAsStringAsync();
                    if (responce.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }

                    var json = Newtonsoft.Json.JsonConvert.DeserializeObject<Json>(str);
                    await DecodeJson(json);

                    responce.EnsureSuccessStatusCode();
                    return true;
                }
            } catch (UnauthorizedAccessException) {
                throw;
            } catch (FormatException) {
                throw;
            } catch {
                return false;
            }
        }
        private async Task DecodeJson(Json data) {
            var personDict = await App.Database.GetDictionary();

            if (!data.user.HasValue || !personDict.ContainsKey(data.user.Value)) 
                throw new FormatException();

            var jsonDict = new Dictionary<int, Json.Row>();
            foreach (var item in data.data) {
                if (!item.person.HasValue) continue;
                jsonDict[item.person.Value] = item;
            }

            var decoded = new HashSet<int>();
            Item DecodeItem(int id) {
                if (!jsonDict.ContainsKey(id)
                    || !personDict.ContainsKey(id)
                    || decoded.Contains(id)
                    ) return null;

                decoded.Add(id);

                return new Item {
                    Person = personDict[id],
                    Mother = jsonDict[id].mother.HasValue ? DecodeItem(jsonDict[id].mother.Value) : null,
                    Father = jsonDict[id].father.HasValue ? DecodeItem(jsonDict[id].father.Value) : null,
                    Siblings = jsonDict[id].siblings
                        .Where(i => i.HasValue && personDict.ContainsKey(i.Value))
                        .Select(i => personDict[i.Value])
                        .ToList()
                };
            }
          

            Root = DecodeItem(data.user.Value);
        }



        



        

    }

}
