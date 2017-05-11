using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Libmemo {
    class PersonDataUploader {

        private readonly string url = Settings.AddPersonUrl;

        public PersonDataUploader() { }

        private Dictionary<string, string> _params = new Dictionary<string, string>();
        public Dictionary<string, string> Params {
            get { return _params; }
        }

        private byte[] _fileData = null;
        public async Task SetFile(ImageSource source) {
            this._fileData = await DependencyService.Get<IImageFileToByteArrayConverter>().Get(source);
        }


        public async Task<bool> Upload() {
            using (var client = new HttpClient()) {
                client.Timeout = new TimeSpan(0, 0, 30);
                using (var content = new MultipartFormDataContent(String.Format("----------{0:N}", Guid.NewGuid()))) {

                    foreach (var item in this.Params) {
                        content.Add(new StringContent(item.Value), item.Key);
                    }

                    if (this._fileData != null) {
                        content.Add(new StreamContent(new MemoryStream(this._fileData)), "photo", "photo.jpg");
                    }

                    try {
                        using (var message = await client.PostAsync(url, content)) {
                            message.EnsureSuccessStatusCode();
                            return true;
                        }
                    } catch (Exception e) {
                        return false;
                    }
                }
            }
        }

    }

}
