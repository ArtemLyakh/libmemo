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

        private int id;
        
        public DetailPageViewModel(int id) {
            this.id = id;
        }

        public async void Init() {
            //var person = await App.Database.GetById<Person>(id);
            Person person = null;
            if (person != null) {
                this.FIO = person.FIO;
                this.LatLon = $"{person.Latitude.ToString(CultureInfo.InvariantCulture)}, {person.Longitude.ToString(CultureInfo.InvariantCulture)}";

                //if (Uri.TryCreate(person.ImageUrl, UriKind.Absolute, out Uri imageUrl))
                //    this.ImageUri = imageUrl;

                if (!string.IsNullOrWhiteSpace(person.Text)) {
                    this.Text = person.Text;
                }

                if (AuthHelper.IsAdmin || AuthHelper.CurrentUserId == person.Owner) {
                    CanEdit = true;
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

                
                //if (Uri.TryCreate(person.SchemeUrl, UriKind.Absolute, out Uri schemeUrl)) {
                //    this.IsSchemeShow = true;
                //} else {
                //    this.IsSchemeShow = false;
                //}
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



        public ICommand EditCommand {
            get => new Command(async () => {
                ContentPage page;
                if (AuthHelper.IsAdmin) {
                    page = new EditPersonPageAdmin(id);
                } else {
                    page = new EditPersonPage(id);
                }
                await App.GlobalPage.Push(page);
            });
        }


        private bool _isHeightShow;
        public bool IsHeightShow {
            get { return this._isHeightShow; }
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

        public ICommand SchemeDownloadCommand {
            get => new Command(async () => {
                //var person = await App.Database.GetById<Person>(id);
                //if (Uri.TryCreate(person.SchemeUrl, UriKind.Absolute, out Uri schemeUrl)) {
                //    Device.OpenUri(schemeUrl);
                //}
            });
        }



        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
