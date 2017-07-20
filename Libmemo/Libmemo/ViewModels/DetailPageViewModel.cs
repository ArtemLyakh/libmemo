using System;
using System.Globalization;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public class DetailPageViewModel : INotifyPropertyChanged {

        private int Id { get; set; }

        public DetailPageViewModel(int id) {
            this.Id = id;
        }

        public ICommand BackCommand => new Command(async () => await App.GlobalPage.Pop());

        public ICommand LoadCommand => new Command(async () => {
            var person = await App.Database.GetById(Id);

            if (person != null) {
                this.FIO = person.FIO;
                this.LatLon = $"{person.Latitude.ToString(CultureInfo.InvariantCulture)}, {person.Longitude.ToString(CultureInfo.InvariantCulture)}";

                if (person.ImageUrl != null) {
                    this.ImageUri = person.ImageUrl;
                } else {
                    this.ImageUri = null;
                }

                if (!string.IsNullOrWhiteSpace(person.Text)) {
                    this.Text = person.Text;
                } else {
                    this.Text = string.Empty;
                }

                if (person.Height.HasValue) {
                    this.IsHeightShow = true;
                    this.Height = person.Height.Value.ToString(CultureInfo.InvariantCulture);
                } else {
                    this.IsHeightShow = false;
                    this.Height = string.Empty;
                }

                if (person.Width.HasValue) {
                    this.IsWidthShow = true;
                    this.Width = person.Width.Value.ToString(CultureInfo.InvariantCulture);
                } else {
                    this.IsWidthShow = false;
                    this.Width = string.Empty;
                }

                if (person.SchemeUrl != null) {
                    this.IsSchemeShow = true;
                    this.SchemeUri = person.SchemeUrl;
                } else {
                    this.IsSchemeShow = false;
                    this.SchemeUri = null;
                }
            }
        });


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

        private bool _isHeightShow;
        public bool IsHeightShow {
            get => this._isHeightShow;
            set {
                if (this._isHeightShow != value) {
                    this._isHeightShow = value;
                    this.OnPropertyChanged(nameof(IsHeightShow));
                }
            }
        }
        private string _height;
        public string Height {
            get { return this._height; }
            set {
                if (this._height != value) {
                    this._height = value;
                    this.OnPropertyChanged(nameof(Height));
                }
            }
        }

        private bool _isWidthShow;
        public bool IsWidthShow {
            get { return this._isWidthShow; }
            set {
                if (this._isWidthShow != value) {
                    this._isWidthShow = value;
                    this.OnPropertyChanged(nameof(IsWidthShow));
                }
            }
        }
        private string _width;
        public string Width {
            get { return this._width; }
            set {
                if (this._width != value) {
                    this._width = value;
                    this.OnPropertyChanged(nameof(Width));
                }
            }
        }

        private bool _isSchemeShow;
        public bool IsSchemeShow {
            get { return this._isSchemeShow; }
            set {
                if (this._isSchemeShow != value) {
                    this._isSchemeShow = value;
                    this.OnPropertyChanged(nameof(IsSchemeShow));
                }
            }
        }


        private Uri SchemeUri { get; set; }

        public ICommand SchemeDownloadCommand => new Command(() => {
            if (this.SchemeUri != null) Device.OpenUri(SchemeUri);
        });




        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
