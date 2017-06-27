using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json;

namespace Libmemo {
    public class PersonDataLoader {

        private readonly Uri uri;

        public PersonDataLoader(string url) {
            this.uri = new Uri(url);
        }
        public PersonDataLoader(Uri uri) {
            this.uri = uri;
        }

        private Dictionary<string, string> _params = new Dictionary<string, string>();
        public Dictionary<string, string> Params {
            get { return _params; }
        }

        private Dictionary<string, Tuple<string, Stream>> _files = new Dictionary<string, Tuple<string, Stream>>();
        public Dictionary<string, Tuple<string, Stream>> Files {
            get => _files;
        }

        private byte[] _fileData = null;
        public async Task SetFile(ImageSource source) {
            this._fileData = await DependencyService.Get<IImageFileToByteArrayConverter>().Get(source);
        }


        public async Task<bool> Upload() {
            try {
                using (var handler = new HttpClientHandler { CookieContainer = AuthHelper.CookieContainer })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(20) })
                using (var content = new MultipartFormDataContent(String.Format("----------{0:N}", Guid.NewGuid()))) {
                    foreach (var item in this.Params) {
                        content.Add(new StringContent(item.Value), item.Key);
                    }

                    foreach (var item in Files) {
                        content.Add(new StreamContent(item.Value.Item2), item.Key, item.Value.Item1);
                    }

                    if (this._fileData != null) {
                        content.Add(new ByteArrayContent(this._fileData), "photo", "photo.jpg");
                    }

                    using (var message = await client.PostAsync(uri, content)) {
                        if (message.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                            throw new UnauthorizedAccessException();
                        }

                        var str = await message.Content.ReadAsStringAsync();
                        message.EnsureSuccessStatusCode();
                        return true;
                    }
                }
            } catch (UnauthorizedAccessException) {
                throw;
            } catch { 
                return false;
            }

        }

        public async Task<PersonData> GetPersonData() {
            try {
                using (var handler = new HttpClientHandler { CookieContainer = AuthHelper.CookieContainer })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) })
                using (var responce = await client.GetAsync(uri)) {

                    if (responce.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }

                    var str = await responce.Content.ReadAsStringAsync();
                    responce.EnsureSuccessStatusCode();
                    var data = JsonConvert.DeserializeObject<PersonDataJson>(str);
                    return new PersonData {
                        FirstName = !string.IsNullOrWhiteSpace(data.firstName) ? data.firstName : null,
                        SecondName = !string.IsNullOrWhiteSpace(data.secondName) ? data.secondName : null,
                        LastName = !string.IsNullOrWhiteSpace(data.lastName) ? data.lastName : null,
                        DateBirth = (DateTime.TryParse(data.dateBirth, out DateTime dateBirth)) ? (DateTime?)dateBirth : null,
                        PhotoUri = (Uri.TryCreate(data.photo, UriKind.Absolute, out Uri photoUri)) ? photoUri : null
                    };
                }

            } catch (UnauthorizedAccessException) {
                throw;
            } catch (Exception) {
                return null;
            }
        }



    }

    public class PersonData {
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateBirth { get; set; }
        public Uri PhotoUri { get; set; }
    }
    public class PersonDataJson {
        public string firstName { get; set; }
        public string secondName { get; set; }
        public string lastName { get; set; }
        public string dateBirth { get; set; }
        public string photo { get; set; }
    }

}
