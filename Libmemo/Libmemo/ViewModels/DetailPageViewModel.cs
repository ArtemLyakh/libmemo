using System;
using System.Globalization;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo {
    public class DetailPageViewModel : INotifyPropertyChanged {

        public DetailPageViewModel(Person person) {
            this.FIO = person.FIO;
            this.LatLon = $"{person.Latitude.ToString(CultureInfo.InvariantCulture)}, {person.Longitude.ToString(CultureInfo.InvariantCulture)}";
            
            if (!string.IsNullOrWhiteSpace(person.ImageUrl) && Uri.TryCreate(person.ImageUrl, UriKind.Absolute, out Uri uri)) {
                this.ImageUri = uri;
            }

            if (!string.IsNullOrWhiteSpace(person.Text)) {
                this.Text = person.Text;
            }

            if (AuthHelper.IsAdmin || AuthHelper.CurrentUserId == 216) {
                CanEdit = true;
            }
        }


        private bool _canEdit;
        public bool CanEdit {
            get { return this._canEdit; }
            set {
                if (this._canEdit != value) {
                    this._canEdit = value;
                    this.OnPropertyChanged(nameof(CanEdit));
                }
            }
        }


        private string _fio;
        public string FIO {
            get { return this._fio; }
            set {
                if (this._fio != value) {
                    this._fio = value;
                    this.OnPropertyChanged(nameof(FIO));
                }
            }
        }

        private string _latLon;
        public string LatLon {
            get { return this._latLon; }
            set {
                if (this._latLon != value) {
                    this._latLon = value;
                    this.OnPropertyChanged(nameof(LatLon));
                }
            }
        }

        private Uri _imageUri;
        public Uri ImageUri {
            get { return this._imageUri; }
            set {
                if (this._imageUri != value) {
                    this._imageUri = value;
                    this.OnPropertyChanged(nameof(ImageUri));
                }
            }
        }

        private string _test;
        public string Text {
            get { return this._test; }
            set {
                if (this._test != value) {
                    this._test = value;
                    this.OnPropertyChanged(nameof(Text));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
