using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo {
    public abstract class BaseAddViewModel : INotifyPropertyChanged {

        protected const double DEFAULT_LATITUDE = 47.23135;
        protected const double DEFAULT_LONGITUDE = 39.72328;
        protected const float DEFAULT_ZOOM = 18;
        protected const long SCHEME_FILE_MAX_SIZE = 2;

        public BaseAddViewModel() {
            GetGPSPermission();
            PersonTypeIndex = 0;
        }

        private bool _gpsPermissionsGained = false;
        private async void GetGPSPermission() {
            await UtilsFunctions.GetGPSPermissionOrExit();

            this._gpsPermissionsGained = true;
            SetGPSTracking(true);
        }
        public void SetGPSTracking(bool track) {
            if (_gpsPermissionsGained) {
                this.MyLocationEnabled = track;
            }
        }



        private enum PersonType {
            Live, Dead
        }
        private Dictionary<string, PersonType> personTypeDictionary = new Dictionary<string, PersonType> {
            { "Мертвый", PersonType.Dead },
            { "Живой", PersonType.Live }
        };
        public List<string> PersonTypeList {
            get => personTypeDictionary.Select(i => i.Key).ToList();
        }

        private int _personTypeIndex;
        public int PersonTypeIndex {
            get => _personTypeIndex;
            set {
                _personTypeIndex = value;
                PersonTypeSelected(value);
            }
        }

        private void PersonTypeSelected(int index) {
            var type = personTypeDictionary[PersonTypeList[index]];
            switch (type) {
                case PersonType.Live:
                    IsDeadPerson = false;
                    break;
                case PersonType.Dead: default:
                    IsDeadPerson = true;
                    break;
            }
        }


        private bool _isDeadPerson = true;
        public bool IsDeadPerson {
            get => _isDeadPerson;
            set {
                _isDeadPerson = value;
                OnPropertyChanged(nameof(IsDeadPerson));
            }
        }



        private string _firstName;
        public string FirstName {
            get { return _firstName; }
            set {
                if (_firstName != value) {
                    _firstName = value;
                    this.OnPropertyChanged(nameof(FirstName));
                }
            }
        }

        private string _secondName;
        public string SecondName {
            get { return _secondName; }
            set {
                if (_secondName != value) {
                    _secondName = value;
                    this.OnPropertyChanged(nameof(SecondName));
                }
            }
        }

        private string _lastName;
        public string LastName {
            get { return _lastName; }
            set {
                if (_lastName != value) {
                    _lastName = value;
                    this.OnPropertyChanged(nameof(LastName));
                }
            }
        }

        private DateTime? _dateBirth = null;
        public DateTime? DateBirth {
            get { return _dateBirth; }
            set {
                if (_dateBirth != value) {
                    _dateBirth = value;
                    this.OnPropertyChanged(nameof(DateBirth));
                }
            }
        }

        private DateTime? _dateDeath = null;
        public DateTime? DateDeath {
            get { return _dateDeath; }
            set {
                if (_dateDeath != value) {
                    _dateDeath = value;
                    this.OnPropertyChanged(nameof(DateDeath));
                }
            }
        }

        private string _text;
        public string Text {
            get { return _text; }
            set {
                if (_text != value) {
                    _text = value;
                    this.OnPropertyChanged(nameof(Text));
                }
            }
        }

        private ImageSource _photoSource;
        public ImageSource PhotoSource {
            get { return _photoSource; }
            set {
                if (_photoSource != value) {
                    _photoSource = value;
                    this.OnPropertyChanged(nameof(PhotoSource));
                }
            }
        }


        private Position _mapCenter = new Position(DEFAULT_LATITUDE, DEFAULT_LONGITUDE);
        public Position MapCenter {
            get { return _mapCenter; }
            set {
                if (_mapCenter != value) {
                    _mapCenter = value;
                    this.OnPropertyChanged(nameof(MapCenter));
                }
            }
        }

        private float _zoom = DEFAULT_ZOOM;
        public float Zoom {
            get { return _zoom; }
            set {
                if (_zoom != value) {
                    _zoom = value;
                    this.OnPropertyChanged(nameof(Zoom));
                }
            }
        }

        private bool _mLocationEnabled = false;
        public bool MyLocationEnabled {
            get { return _mLocationEnabled; }
            protected set {
                if (_mLocationEnabled != value) {
                    _mLocationEnabled = value;
                    this.OnPropertyChanged(nameof(MyLocationEnabled));
                }
            }
        }

        private Position? _userPosition = null;
        public Position? UserPosition {
            get { return this._userPosition; }
            protected set {
                if (this._userPosition != value) {
                    this._userPosition = value;
                    this.OnPropertyChanged(nameof(UserPosition));
                    this.OnPropertyChanged(nameof(Latitude));
                    this.OnPropertyChanged(nameof(Longitude));
                }
            }
        }
        public string Latitude {
            get { return this.UserPosition?.Latitude.ToString() ?? char.ConvertFromUtf32(0x2014); }
        }
        public string Longitude {
            get { return this.UserPosition?.Longitude.ToString() ?? char.ConvertFromUtf32(0x2014); }
        }


        private double? _height;
        public double? Height {
            get { return _height; }
            set {
                if (_height != value) {
                    _height = value;
                    this.OnPropertyChanged(nameof(Height));
                }
            }
        }
        private double? _width;
        public double? Width {
            get { return _width; }
            set {
                if (_width != value) {
                    _width = value;
                    this.OnPropertyChanged(nameof(Width));
                }
            }
        }


        protected Stream SchemeStream { get; set; }
        private string _schemeName;
        public string SchemeName {
            get => string.IsNullOrWhiteSpace(_schemeName) ? "Не выбрано" : _schemeName;
            private set {
                if (_schemeName != value) {
                    _schemeName = value;
                    OnPropertyChanged(nameof(SchemeName));
                }
            }
        }
        private void SetScheme(string name, Stream stream) {
            SchemeName = name;
            SchemeStream = stream;
        }
        private void ResetScheme() {
            SchemeName = null;

            SchemeStream?.Dispose();
            SchemeStream = null;
        }

        public ICommand SelectSchemeCommand {
            get => new Command(async () => {
                var storage = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                if (storage != PermissionStatus.Granted) {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Storage });
                    var status = results[Permission.Storage];
                    if (status != PermissionStatus.Granted) {
                        Device.BeginInvokeOnMainThread(async () =>
                            await App.Current.MainPage.DisplayAlert("Ошибка", "Необходимо разрешение для чтения файла", "Ок"));
                        return;
                    }
                }

                try {
                    var file = await Plugin.FilePicker.CrossFilePicker.Current.PickFile();
                    SchemeStream?.Dispose();

                    var stream = DependencyService.Get<IFileStreamPicker>().GetStream(file.FilePath);
                    if (stream.Length > SCHEME_FILE_MAX_SIZE * 1024 * 1024) {
                        Device.BeginInvokeOnMainThread(async () =>
                            await App.Current.MainPage.DisplayAlert("Ошибка", $"Размер файла не должен превышать {SCHEME_FILE_MAX_SIZE} МБ ({stream.Length/1024/1024} МБ)", "Ок"));
                        return;
                    }
                    SetScheme(file.FileName, stream);
                } catch {
                    Device.BeginInvokeOnMainThread(async () => 
                        await App.Current.MainPage.DisplayAlert("Ошибка", "Возникла ошибка при выборе файла", "Ок"));
                }

            });
        }








        public ICommand UserPositionChangedCommand {
            get => new Command<Position>((Position position) => {
                this.MapCenter = position;
                this.UserPosition = position;
            });
        }

        public ICommand PickPhotoCommand {
            get => new Command(async () => {
                if (CrossMedia.Current.IsPickPhotoSupported) {
                    var photo = await CrossMedia.Current.PickPhotoAsync();
                    if (photo == null) return;
                    this.PhotoSource = ImageSource.FromFile(photo.Path);
                }
            });
        }

        public ICommand MakePhotoCommand {
            get => new Command(async () => {
                if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported) {
                    var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { SaveToAlbum = false });
                    if (file == null) return;
                    PhotoSource = ImageSource.FromFile(file.Path);
                }
            });
        }

        public ICommand ResetCommand {
            get => new Command(ResetFields);
        }
        protected virtual void ResetFields() {
            this.LastName = null;
            this.FirstName = null;
            this.SecondName = null;

            this.DateBirth = null;
            this.DateDeath = null;

            this.Text = null;
            this.PhotoSource = null;

            this.Height = null;
            this.Width = null;
            ResetScheme();
        }

        public ICommand SendCommand {
            get => new Command(Send);
        }
        protected abstract void Send();



        protected virtual IEnumerable<string> Validate() {
            if (this.IsDeadPerson && !this.UserPosition.HasValue) yield return "Ошибка определения местоположения";
            if (String.IsNullOrWhiteSpace(this.FirstName)) yield return "Поле \"Имя\" не заполнено";
        }

        protected virtual async Task AddParams(PersonDataLoader uploader) {
            uploader.Params.Add("type", this.IsDeadPerson ? "dead" : "live");

            if (this.IsDeadPerson) {
                uploader.Params.Add("latitude", this.UserPosition.Value.Latitude.ToString(CultureInfo.InvariantCulture));
                uploader.Params.Add("longitude", this.UserPosition.Value.Longitude.ToString(CultureInfo.InvariantCulture));
            }        

            uploader.Params.Add("first_name", this.FirstName.ToString());

            if (!string.IsNullOrWhiteSpace(this.SecondName)) {
                uploader.Params.Add("second_name", this.SecondName.ToString());
            }
            if (!string.IsNullOrWhiteSpace(this.LastName)) {
                uploader.Params.Add("last_name", this.LastName.ToString());
            }

            if (this.DateBirth.HasValue) {
                uploader.Params.Add("date_birth", this.DateBirth.Value.ToString("yyyy-MM-dd"));
            }
            if (this.DateDeath.HasValue) {
                uploader.Params.Add("date_death", this.DateDeath.Value.ToString("yyyy-MM-dd"));
            }

            if (this.PhotoSource != null) {
                await uploader.SetFile(this.PhotoSource);
                //var path = ((FileImageSource)PhotoSource).File;
                //uploader.Files.Add("photo", Tuple.Create(Path.GetFileName(path), DependencyService.Get<IFileStreamPicker>().GetStream(path)));
            }

            if (this.IsDeadPerson) {
                if (!string.IsNullOrWhiteSpace(this.Text)) {
                    uploader.Params.Add("text", this.Text.ToString());
                }
                if (this.Height.HasValue) {
                    uploader.Params.Add("height", this.Height.Value.ToString(CultureInfo.InvariantCulture));
                }
                if (this.Width.HasValue) {
                    uploader.Params.Add("width", this.Width.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (this.SchemeStream != null) {
                    uploader.Files.Add("scheme", Tuple.Create(this.SchemeName, this.SchemeStream));
                }
            }

        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
