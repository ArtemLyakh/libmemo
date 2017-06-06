using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo {
    public class EditPersonPageViewModel : INotifyPropertyChanged {

        private int id;
        private bool isChanged = false;
        public EditPersonPageViewModel(int id) {
            this.id = id;
            GetGPSPermission();
            Init();
        }

        #region Owner
        public bool IsOwnerVisible { get => AuthHelper.IsAdmin; }
        private User _owner;
        public User Owner {
            get => _owner;
            set {
                if (_owner != value) {
                    _owner = value;
                    isChanged = true;
                    this.OnPropertyChanged(nameof(Owner));
                    this.OnPropertyChanged(nameof(OwnerText));
                }
            }
        }
        public string OwnerText { get => this.Owner == null ? "Не выбрано" : $"{this.Owner.Id}: {this.Owner.FIO}"; }
        public ICommand SelectOwnerCommand {
            get => new Command(async () => {
                var searchPage = new SearchPage(await App.Database.GetItems<User>());
                searchPage.ItemSelected += async (sender, id) => {
                    this.Owner = await App.Database.GetById<User>(id);
                };

                await App.CurrentNavPage.Navigation.PushAsync(searchPage);
            });
        }
        #endregion


        #region Position

        #region Constants
        private const double DEFAULT_LATITUDE = 47.23135;
        private const double DEFAULT_LONGITUDE = 39.72328;
        private const float DEFAULT_ZOOM = 18;
        #endregion

        #region GPS Permissions

        private bool _gpsPermissionsGained = false;

        public void SetGPSTracking(bool track) {
            if (_gpsPermissionsGained) {
                this.MyLocationEnabled = track;
            }
        }

        private async void GetGPSPermission() {
            await UtilsFunctions.GetGPSPermissionOrExit();

            this._gpsPermissionsGained = true;
            SetGPSTracking(true);
        }

        #endregion

        #region Map

        public INativeMapFunction MapFunctions { get; set; }

        ObservableCollection<CustomPin> _customPins = new ObservableCollection<CustomPin>();
        public ObservableCollection<CustomPin> CustomPins {
            get { return _customPins; }
            set {
                if (_customPins != value) {
                    _customPins = value;
                    this.OnPropertyChanged(nameof(CustomPins));
                }
            }
        }

        #region Properties

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
            private set {
                if (_mLocationEnabled != value) {
                    _mLocationEnabled = value;
                    this.OnPropertyChanged(nameof(MyLocationEnabled));
                }
            }
        }

        private Position? _userPosition = null;
        public Position? UserPosition {
            get { return this._userPosition; }
            private set {
                if (this._userPosition != value) {
                    this._userPosition = value;
                    this.OnPropertyChanged(nameof(UserPosition));
                }
            }
        }

        public string PersonLatLon {
            get => $"{PersonPosition.Latitude}\n{PersonPosition.Longitude}";
        }

        private Position _personPosition;
        public Position PersonPosition {
            get { return _personPosition; }
            set {
                if (_personPosition != value) {
                    isChanged = true;

                    _personPosition = value;

                    var pin = CustomPins.FirstOrDefault();
                    if (pin != null) pin.Position = value;

                    OnPropertyChanged(nameof(PersonLatLon));
                }
            }
        }

        #endregion

        #region Commands

        private bool firstTimeUserPosition = true;
        public ICommand UserPositionChangedCommand {
            get {
                return new Command<Position>((Position position) => {
                    if (firstTimeUserPosition) {
                        firstTimeUserPosition = false;
                        this.MapCenter = position;
                    }
                    this.UserPosition = position;
                });
            }
        }

        public ICommand MapClickCommand {
            get => new Command<Position>((Position position) => {
                PersonPosition = position;
            });
        }

        public ICommand CurrentGeoCommand {
            get => new Command(() => {
                if (UserPosition.HasValue) {
                    PersonPosition = UserPosition.Value;
                }
            });
        }

        #endregion

        #endregion

        #endregion


        private bool _isLatLonShow;
        public bool IsLatLonShow {
            get { return _isLatLonShow; }
            set {
                if (_isLatLonShow != value) {
                    _isLatLonShow = value;
                    if (value) _buttonShowHide = "Скрыть";
                    else _buttonShowHide = "Редактировать";
                    this.OnPropertyChanged(nameof(ButtonShowHide));
                    this.OnPropertyChanged(nameof(IsLatLonShow));
                }
            }
        }
        private string _buttonShowHide = "Редактировать";
        public string ButtonShowHide {
            get => _buttonShowHide;
        }


        public ICommand ButtonShowHideClickCommand {
            get => new Command(() => IsLatLonShow = !IsLatLonShow);
        }

        




        public async void Init() {
            var person = await App.Database.GetById<Person>(id);

            FirstName = person.FirstName;
            SecondName = person.SecondName;
            LastName = person.LastName;
            DateBirth = person.DateBirth;
            DateDeath = person.DateDeath;
            Text = person.Text;
            if (Uri.TryCreate(person.ImageUrl, UriKind.Absolute, out Uri imageUri))
                PhotoSource = new UriImageSource() { CachingEnabled = true, Uri = imageUri };

            CustomPins.Clear();
            CustomPins.Add(new CustomPin() {
                PinImage = PinImage.Default,
                Id = person.Id.ToString(),
                Position = new Position(person.Latitude, person.Longitude),
                Visible = true
            });
            PersonPosition = new Position(person.Latitude, person.Longitude);

            if (AuthHelper.IsAdmin) {
                var owner = (await App.Database.GetItems<User>()).FirstOrDefault(i => i.Owner == person.Owner);
                if (owner != null) {
                    Owner = owner;
                }
            }


            isChanged = false;
        }











        private string _firstName;
        public string FirstName {
            get { return _firstName; }
            set {
                if (_firstName != value) {
                    _firstName = value;
                    this.OnPropertyChanged(nameof(FirstName));
                    isChanged = true;
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
                    isChanged = true;
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
                    isChanged = true;
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
                    isChanged = true;
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
                    isChanged = true;
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
                    isChanged = true;
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
                    isChanged = true;
                }
            }
        }

        public ICommand PickPhotoCommand {
            get {
                return new Command(async () => {
                    if (CrossMedia.Current.IsPickPhotoSupported) {
                        MediaFile photo = await CrossMedia.Current.PickPhotoAsync();
                        if (photo == null) return;
                        this.PhotoSource = ImageSource.FromFile(photo.Path);
                    }
                });
            }
        }

        public ICommand MakePhotoCommand {
            get {
                return new Command(async () => {
                    if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported) {
                        string fio = String.Join("_", new string[] { this.LastName, this.FirstName, this.SecondName }).Trim();
                        fio = String.IsNullOrWhiteSpace(fio)
                            ? $"{DateTime.Now.ToString("dd.MM.yyyy_hh.mm.ss")}.jpg"
                            : fio + ".jpg";

                        MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions {
                            SaveToAlbum = true,
                            Directory = "RitualUg",
                            Name = fio
                        });

                        if (file == null)
                            return;

                        PhotoSource = ImageSource.FromFile(file.Path);
                    }
                });
            }
        }

        public ICommand ResetCommand {
            get => new Command(() => Init());
        }











        #region SendCommand

        public ICommand SendCommand {
            get {
                return new Command(async () => {
                    if (!isChanged) {
                        App.ToastNotificator.Show("Отсутствуют изменения");
                        return;
                    }

                    var errors = string.Join("\n", ValidateSend());

                    if (!string.IsNullOrWhiteSpace(errors)) {
                        Device.BeginInvokeOnMainThread(async () => {
                            await App.Current.MainPage.DisplayAlert("Ошибка", errors, "ОК");
                        });
                        return;
                    }

                    App.ToastNotificator.Show("Отправка на сервер");

                    PersonDataLoader uploader = new PersonDataLoader(Settings.EditPersonUrl);
                    await AddParams(uploader);

                    try {
                        var success = await uploader.Upload();

                        if (success) {
                            UploadSucceeded();
                        } else {
                            Device.BeginInvokeOnMainThread(async () => {
                                await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка отправки данных", "ОК");
                            });
                        }
                    } catch (UnauthorizedAccessException) {
                        AuthHelper.Relogin();
                    }

                });
            }
        }

        private IEnumerable<string> ValidateSend() {
            if (String.IsNullOrWhiteSpace(this.FirstName)) yield return "Поле \"Имя\" не заполнено";
            if (AuthHelper.IsAdmin && Owner == null) yield return "Поле \"Владелец\" не заполнено";
        }

        private async Task AddParams(PersonDataLoader uploader) {
            uploader.Params.Add("id", this.id.ToString());

            uploader.Params.Add("latitude", this.PersonPosition.Latitude.ToString(CultureInfo.InvariantCulture));
            uploader.Params.Add("longitude", this.PersonPosition.Longitude.ToString(CultureInfo.InvariantCulture));
            uploader.Params.Add("first_name", this.FirstName.ToString());

            if (AuthHelper.IsAdmin) {
                uploader.Params.Add("owner", this.Owner.Id.ToString());
            }         

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
            if (!string.IsNullOrWhiteSpace(this.Text)) {
                uploader.Params.Add("text", this.Text.ToString());
            }
            if (this.PhotoSource != null && this.PhotoSource is FileImageSource) {
                await uploader.SetFile(this.PhotoSource);
            }
        }

        private void UploadSucceeded() {
            App.ToastNotificator.Show("Данные успешно отправлены");
            App.Database.Load();
            App.MenuPage.ExecuteMenuItem(MenuItemId.Map);
        }

        #endregion














        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
