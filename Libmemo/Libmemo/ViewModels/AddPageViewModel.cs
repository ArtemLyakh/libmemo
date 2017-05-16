using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo {
    class AddPageViewModel : INotifyPropertyChanged {




        public AddPageViewModel() {
            GetGPSPermission();
        }

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

        #endregion

        #region Commands

        public ICommand UserPositionChangedCommand {
            get {
                return new Command<Position>((Position position) => {
                    this.MapCenter = position;
                    this.UserPosition = position;
                });
            }
        }

        #endregion

        #endregion

        #endregion



























        #region Properties

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
        #endregion

        #region Commands

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
            get {
                return new Command(() => {
                    this.LastName = null;
                    this.FirstName = null;
                    this.SecondName = null;

                    this.DateBirth = null;
                    this.DateDeath = null;

                    this.Text = null;
                    this.PhotoSource = null;
                });
            }
        }

        #region SendCommand

        public ICommand SendCommand {
            get {
                return new Command(async () => {
                    List<string> errors = new List<string>();
                    if (!this.UserPosition.HasValue) errors.Add("Ошибка определения местоположения");
                    if (String.IsNullOrWhiteSpace(this.FirstName)) errors.Add("Поле \"Имя\" не заполнено");

                    if (errors.Count > 0) {
                        Device.BeginInvokeOnMainThread(async () => {
                            await App.Current.MainPage.DisplayAlert("Ошибка", String.Join("\n", errors), "ОК");
                        });
                        return;
                    }

                    PersonDataUploader uploader = new PersonDataUploader();
                    uploader.Params.Add("latitude", this.UserPosition.Value.Latitude.ToString(CultureInfo.InvariantCulture));
                    uploader.Params.Add("longitude", this.UserPosition.Value.Longitude.ToString(CultureInfo.InvariantCulture));
                    uploader.Params.Add("first_name", this.FirstName.ToString());

                    if (!String.IsNullOrWhiteSpace(this.SecondName)) {
                        uploader.Params.Add("second_name", this.SecondName.ToString());
                    }
                    if (!String.IsNullOrWhiteSpace(this.LastName)) {
                        uploader.Params.Add("last_name", this.LastName.ToString());
                    }

                    if (this.DateBirth.HasValue) {
                        uploader.Params.Add("date_birth", this.DateBirth.Value.ToString("yyyy-MM-dd"));
                    }
                    if (this.DateDeath.HasValue) {
                        uploader.Params.Add("date_death", this.DateDeath.Value.ToString("yyyy-MM-dd"));
                    }
                    if (!String.IsNullOrWhiteSpace(this.Text)) {
                        uploader.Params.Add("text", this.Text.ToString());
                    }
                    if (this.PhotoSource != null) {
                        await uploader.SetFile(this.PhotoSource);
                    }

                    App.ToastNotificator.Show("Отправка на сервер");
                    var success = await uploader.Upload();
                    if (success) {
                        UploadSucceeded();
                    } else {
                        Device.BeginInvokeOnMainThread(async () => {
                            await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка отправки данных", "ОК");
                        });
                    }
                });
            }
        }

        private void UploadSucceeded() {
            App.ToastNotificator.Show("Данные успешно отправлены");
            ResetCommand.Execute(null);
        }

        #endregion

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }

}