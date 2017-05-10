using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo {
    public class MainPageViewModel : INotifyPropertyChanged {

        #region Constants
        private const double DEFAULT_LATITUDE = 47.23135;
        private const double DEFAULT_LONGITUDE = 39.72328;
        private const float DEFAULT_ZOOM = 18;

        private const double PLAY_DISTANCE = 15;
        private const double ROUTE_END_DISTANCE = 10;
        #endregion

        #region Constructor
        public MainPageViewModel(Page page) {

            #region Pin load
            InitPinsFromMemory();
            StartLoadingPinsFromServer();
            #endregion


            #region Permissions
            GetPermission();
            #endregion


            #region TextToSpeechEvents
            App.TextToSpeech.OnStart += TextToSpeech_OnStart;
            App.TextToSpeech.OnEnd += TextToSpeech_OnEnd;
            #endregion

        }

        private bool _gpsPermissionsGained = false;
        public void SetGPSTracking(bool track) {
            if (_gpsPermissionsGained) {
                this.MyLocationEnabled = track;
            }
        }

        #region Permissions
        private async void GetPermission() {
            var location = await Plugin.Permissions.CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Location);
            if (location == Plugin.Permissions.Abstractions.PermissionStatus.Granted) {
                this.MyLocationEnabled = true;
                this._gpsPermissionsGained = true;
            } else {
                var results = await Plugin.Permissions.CrossPermissions.Current.RequestPermissionsAsync(new[] { Plugin.Permissions.Abstractions.Permission.Location });
                var status = results[Plugin.Permissions.Abstractions.Permission.Location];
                if (status == Plugin.Permissions.Abstractions.PermissionStatus.Granted) {
                    this.MyLocationEnabled = true;
                    this._gpsPermissionsGained = true;
                } else {
                    Device.BeginInvokeOnMainThread(async () => {
                        await App.Current.MainPage.DisplayAlert("Ошибка", "Приложению требуется доступ к геолокации для функционирования", "Завершить работу");
                        DependencyService.Get<ICloseApplication>().CloseApplication();
                    });
                }
            }
        }
        #endregion

        #endregion




        private Position UserPosition { get; set; }


        public INativeMapFunction MapFunctions { get; set; }

        #region Utils

        #region Pins

        #region Pin initialization

        #region Set pins to map from database
        private async void InitPinsFromMemory() {
            var list = await App.Database.GetItems();

            if (list.Count() > 0) {
                var buf = new ObservableCollection<CustomPin>();

                foreach (var item in list) {
                    var pin = CreatePin(item);
                    buf.Add(pin);
                }

                CustomPins = buf;
            }
        }
        #endregion

        private CustomPin CreatePin(Person person) {
            return new CustomPin() {
                Id = person.Id.ToString(),
                PinImage = string.IsNullOrWhiteSpace(person.Text) ? PinImage.Default : PinImage.Speaker,
                Position = new Position(person.Latitude, person.Longitude),
                Title = person.Name,
                Text = person.DateBirth.HasValue && person.DateDeath.HasValue ? $"{person.DateBirth.Value.Date.ToString("dd.MM.yyyy")}\u2014{person.DateDeath.Value.ToString("dd.MM.yyyy")}" : "",
                Visible = true
            };
        }

        #region Start loading pins from server and set them to map
        private void StartLoadingPinsFromServer() {
            App.Database.LoadSuccess += Database_LoadSuccess;
            App.Database.LoadFail += Database_LoadFail;
            App.Database.Load();
        }

        private void Database_LoadSuccess() {
            App.Database.LoadSuccess -= Database_LoadSuccess;
            App.Database.LoadFail -= Database_LoadFail;
            InitPinsFromMemory();
            App.ToastNotificator.Show("Данные с сервера получены");
        }

        private void Database_LoadFail() {
            App.Database.LoadSuccess -= Database_LoadSuccess;
            App.Database.LoadFail -= Database_LoadFail;
            Device.BeginInvokeOnMainThread(async () => {
                bool again = await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка загрузки данных с сервера", "Повторить", "Отмена");
                if (again) {
                    StartLoadingPinsFromServer();
                }
            });
        }
        #endregion

        #endregion

        #endregion



        #endregion



        #region Map

        #region Properties
        #region MapCenter
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
        #endregion
        #region Zoom
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
        #endregion
        #region IsCameraAnimated
        private bool _isCameraAnimated = true;
        public bool IsCameraAnimated {
            get { return _isCameraAnimated; }
            set {
                if (_isCameraAnimated != value) {
                    _isCameraAnimated = value;
                    this.OnPropertyChanged(nameof(IsCameraAnimated));
                }
            }
        }
        #endregion

        #region IsRotateGesturesEnabled
        private bool _isRotateGesturesEnabled = false;
        public bool IsRotateGesturesEnabled {
            get { return _isRotateGesturesEnabled; }
            set {
                if (_isRotateGesturesEnabled != value) {
                    _isRotateGesturesEnabled = value;
                    this.OnPropertyChanged(nameof(IsRotateGesturesEnabled));
                }
            }
        }
        #endregion
        #region IsScrollGesturesEnabled
        private bool _isScrollGesturesEnabled = false;
        public bool IsScrollGesturesEnabled {
            get { return _isScrollGesturesEnabled; }
            set {
                if (_isScrollGesturesEnabled != value) {
                    _isScrollGesturesEnabled = value;
                    this.OnPropertyChanged(nameof(IsScrollGesturesEnabled));
                }
            }
        }
        #endregion
        #region IsTiltGesturesEnabled
        private bool _isTiltGesturesEnabled = false;
        public bool IsTiltGesturesEnabled {
            get { return _isTiltGesturesEnabled; }
            set {
                if (_isTiltGesturesEnabled != value) {
                    _isTiltGesturesEnabled = value;
                    this.OnPropertyChanged(nameof(IsTiltGesturesEnabled));
                }
            }
        }
        #endregion
        #region IsZoomGesturesEnabled
        private bool _isZoomGesturesEnabled = false;
        public bool IsZoomGesturesEnabled {
            get { return _isZoomGesturesEnabled; }
            set {
                if (_isZoomGesturesEnabled != value) {
                    _isZoomGesturesEnabled = value;
                    this.OnPropertyChanged(nameof(IsZoomGesturesEnabled));
                }
            }
        }
        #endregion

        #region SelectedPin
        private CustomPin _selectedPin = null;
        public CustomPin SelectedPin {
            get { return _selectedPin; }
            set {
                if (_selectedPin != value) {
                    _selectedPin = value;
                    this.OnPropertyChanged(nameof(SelectedPin));
                }
            }
        }
        #endregion

        #region CustomPins
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
        #endregion

        #region MyLocationEnabled
        private bool _mLocationEnabled = false;
        public bool MyLocationEnabled {
            get { return _mLocationEnabled; }
            set {
                if (_mLocationEnabled != value) {
                    _mLocationEnabled = value;
                    this.OnPropertyChanged(nameof(MyLocationEnabled));
                }
            }
        }
        #endregion
        #endregion

        #region Commands
        public ICommand InfoWindowClickedCommand {
            get {
                return new Command<CustomPin>(async (CustomPin pin) => {
                    var url = (await App.Database.GetById(int.Parse(pin.Id)))?.Link;

                    if (Uri.TryCreate(url, UriKind.Absolute, out Uri res)) {
                        Device.OpenUri(new Uri(url));
                    }
                });
            }
        }
        public ICommand UserPositionChangedCommand {
            get {
                return new Command<Position>((Position position) => {
                    UserPositionChanged(position);
                });
            }
        }
        #endregion

        #endregion

        #region Search

        #region Properties
        private string _searchText;
        public string SearchText {
            get { return _searchText; }
            set {
                if (_searchText != value) {
                    _searchText = value;
                    this.OnPropertyChanged(nameof(SearchText));
                }
            }
        }
        #endregion 

        #region Commands
        public ICommand SearchCommand {
            get {
                return new Command(async () => {
                    if (string.IsNullOrWhiteSpace(this.SearchText)) return;

                    var searchPage = new SearchPage(this.SearchText, OnItemSelected, OnSearchChanged);
                    this.SelectedPin = null;
                    await Application.Current.MainPage.Navigation.PushAsync(searchPage);
                });
            }
        }

        public ICommand ResetCommand {
            get {
                return new Command(() => {
                    this.SearchText = "";
                });
            }
        }
        #endregion

        #region Search pages callbacks
        private void OnSearchChanged(object sender, string e) {
            this.SearchText = e;
        }

        private void OnItemSelected(object sender, Person person) {
            var pin = _customPins.FirstOrDefault(e => e.Id == person.Id.ToString());

            if (pin != null) {
                this.SearchText = person.Name;
                this.SelectedPin = pin;
                this.FollowUser = false;
                MoveCameraToPosition(pin.Position);
            }
        }
        #endregion

        #endregion

        #region Route

        #region Commands

        #region Properties
        private Position? RouteFrom { get; set; }
        private Position? RouteTo { get; set; }
        #endregion

        public Command SetLinearRouteCommand {
            get {
                return new Command(() => {
                    if (this.UserPosition != default(Position) && this.SelectedPin != null) {
                        this.RouteFrom = this.UserPosition;
                        this.RouteTo = this.SelectedPin.Position;
                        this?.MapFunctions.SetLinearRoute(this.UserPosition, this.SelectedPin.Position);
                    }
                });
            }
        }
        private bool _calculatedRouteProcessing = false;
        public Command SetCalculatedRouteCommand {
            get {
                return new Command(() => {
                    if (this._calculatedRouteProcessing) return;
                    if (this.UserPosition != default(Position) && this.SelectedPin != null) {
                        this.RouteFrom = this.UserPosition;
                        this.RouteTo = this.SelectedPin.Position;
                        this.MapFunctions.SetCalculatedRoute(this.UserPosition, this.SelectedPin.Position);
                        this._calculatedRouteProcessing = true;
                    }
                });
            }
        }

        #region RouteInitializingSucceedCommand
        public ICommand RouteInitializingSucceedCommand {
            get {
                return new Command(() => {
                    this.IsRouteActive = true;
                    this._calculatedRouteProcessing = false;
                });
            }
        }
        #endregion

        #region RouteInitializingFailedCommand
        public ICommand RouteInitializingFailedCommand {
            get {
                return new Command<CustomPin>(pin => {
                    this._calculatedRouteProcessing = false;
                    App.ToastNotificator.Show("Ошибка построения маршрута");
                });
            }
        }
        #endregion

        #endregion


        #endregion

        #region GPS
        private void UserPositionChanged(Position position) {
            this.UserPosition = position;
            if (this.FollowUser) {
                MoveCameraToUserPosition();
            }
        }

        private void MoveCameraToUserPosition() {
            this.MapCenter = this.UserPosition;
        }

        private void MoveCameraToPosition(Position position) {
            this.MapCenter = position;
        }


        #endregion

        #region FollowUser
        private bool _followUser = true;
        public bool FollowUser {
            get { return this._followUser; }
            set {
                this._followUser = value;
                this.IsScrollGesturesEnabled = !value;
                this.IsRotateGesturesEnabled = !value;
                this.IsTiltGesturesEnabled = !value;
                this.IsZoomGesturesEnabled = !value;
                this.OnPropertyChanged(nameof(FollowUser));
            }
        }
        public ICommand FollowUserToogleCommand {
            get {
                return new Command(() => {
                    if (FollowUser) {
                        FollowUser = false;
                    } else {
                        FollowUser = true;
                        MoveCameraToUserPosition();
                    }
                });
            }
        }
        #endregion

        #region IsRouteActicve
        private bool _isRouteActive = false;
        public bool IsRouteActive {
            get { return this._isRouteActive; }
            set {
                if (this._isRouteActive != value) {
                    this._isRouteActive = value;
                    this.OnPropertyChanged(nameof(IsRouteActive));
                }
            }
        }
        #endregion

        public ICommand DeleteRouteCommand {
            get {
                return new Command(() => {
                    this.MapFunctions.DeleteRoute();
                    this.IsRouteActive = false;
                });
            }
        }






        private bool _ttsStarted = false;
        public ICommand StartTTSOnSelectedPinCommand {
            get {
                return new Command(async () => {
                    if (this._ttsStarted) return;
                    this._ttsStarted = true;
                    var person = await App.Database.GetById(int.Parse(this.SelectedPin.Id));
                    SpeakPersonText(person);
                });
            }
        }
        public ICommand StopTTSCommand {
            get {
                return new Command(() => {
                    StopSpeakPersonText();
                });
            }
        }


        private void TextToSpeech_OnEnd(object sender, string e) {
            _ttsStarted = false;
            CurrentPlayed = null;
        }

        private void TextToSpeech_OnStart(object sender, string e) {
            _ttsStarted = false;
            CurrentPlayed = int.Parse(e);
        }

        private int? _currentPlayed = null;
        public int? CurrentPlayed {
            get { return _currentPlayed; }
            set {
                if (_currentPlayed != value) {
                    _currentPlayed = value;
                    this.OnPropertyChanged(nameof(CurrentPlayed));
                }
            }
        }

        private void SpeakPersonText(Person person) {
            App.TextToSpeech.Speak(person.Text, person.Id);
        }
        private void StopSpeakPersonText() {
            App.TextToSpeech.Stop();
        }







        public ICommand AddNewPersonCommand {
            get {
                return new Command(async () => {
                    if (this.UserPosition == default(Position)) return;

                    var addPage = new AddPage(this.UserPosition, OnItemAdded);
                    await Application.Current.MainPage.Navigation.PushAsync(addPage);
                });
            }
        }
        private void OnItemAdded(object sender, object e) {
            this.MapFunctions.DeleteRoute();
            this.IsRouteActive = false;
            this.SelectedPin = null;
            StartLoadingPinsFromServer();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }


}
