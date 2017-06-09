using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo {
    public class MapPageViewModel : INotifyPropertyChanged {

        #region Constants
        private const double DEFAULT_LATITUDE = 47.23135;
        private const double DEFAULT_LONGITUDE = 39.72328;
        private const float DEFAULT_ZOOM = 18;
        #endregion

        public MapPageViewModel() {

            InitPinsFromMemory();

            GetGPSPermission();

            Zoom = DEFAULT_ZOOM;
            MapCenter = new Position(DEFAULT_LATITUDE, DEFAULT_LONGITUDE);
        }

        public void StartListen() {
            //TTS listeners
            App.TextToSpeech.OnStart += TextToSpeech_OnStart;
            App.TextToSpeech.OnEnd += TextToSpeech_OnEnd;

            //DB listener
            App.Database.LoadSuccess += Database_LoadSuccess;
        }

        public void StopListen() {
            //TTS listeners
            App.TextToSpeech.OnStart -= TextToSpeech_OnStart;
            App.TextToSpeech.OnEnd -= TextToSpeech_OnEnd;

            //DB listener
            App.Database.LoadSuccess -= Database_LoadSuccess;
        }



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

        public INativeMapFunction MapFunctions { get; set; }

        #region Pins

        #region Properties

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

        #region Commands

        public ICommand InfoWindowClickedCommand {
            get => new Command<CustomPin>(async (CustomPin pin) => {
                if (int.TryParse(pin.Id, out int id)) {
                    var page = new DetailPage(id);
                    await App.GlobalPage.Push(page);
                }
            });

        }

        #endregion

        private CustomPin CreatePin(Person person) {
            return new CustomPin() {
                Id = person.Id.ToString(),
                PinImage = string.IsNullOrWhiteSpace(person.Text) ? PinImage.Default : PinImage.Speaker,
                Position = new Position(person.Latitude, person.Longitude),
                Title = person.FIO,
                Text = person.DateBirth.HasValue && person.DateDeath.HasValue ? $"{person.DateBirth.Value.Date.ToString("dd.MM.yyyy")}\u2014{person.DateDeath.Value.ToString("dd.MM.yyyy")}" : "",
                Visible = true,
                Base64 = person.Icon
            };
        }

        #endregion

        #region Camera

        #region Properties

        private Position _mapCenter;
        public Position MapCenter {
            get { return _mapCenter; }
            set {
                if (_mapCenter != value) {
                    _mapCenter = value;
                    this.OnPropertyChanged(nameof(MapCenter));
                }
            }
        }

        private float _zoom;
        public float Zoom {
            get { return _zoom; }
            set {
                if (_zoom != value) {
                    _zoom = value;
                    this.OnPropertyChanged(nameof(Zoom));
                }
            }
        }

        private bool _isCameraAnimated = false;
        public bool IsCameraAnimated {
            get { return _isCameraAnimated; }
            set {
                if (_isCameraAnimated != value) {
                    _isCameraAnimated = value;
                    this.OnPropertyChanged(nameof(IsCameraAnimated));
                }
            }
        }

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

        private void MoveCameraToUserPosition() {
            MoveCameraToPosition(this.UserPosition);
        }

        private void MoveCameraToPosition(Position position) {
            this.MapCenter = position;
        }

        #endregion

        #region GPS

        private Position UserPosition { get; set; }

        #region Properties

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

        #endregion

        #region Commands

        private bool firstCamera = true;
        public ICommand UserPositionChangedCommand {
            get {
                return new Command<Position>((Position position) => {
                    this.UserPosition = position;

                    if (this.FollowUser) {
                        if (firstCamera) {
                            this.IsCameraAnimated = true;
                            firstCamera = false;
                        }
                        MoveCameraToUserPosition();
                    }
                });
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

        #endregion

        #region Routes

        #region Properties

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

        private Position? RouteFrom { get; set; }
        private Position? RouteTo { get; set; }

        #region Commands

        private bool _routeProcessing = false;

        public Command SetLinearRouteCommand {
            get {
                return new Command(() => {
                    if (this._routeProcessing) return;
                    if (this.UserPosition != default(Position) && this.SelectedPin != null) {
                        this.RouteFrom = this.UserPosition;
                        this.RouteTo = this.SelectedPin.Position;
                        this._routeProcessing = true;
                        this.MapFunctions.SetLinearRoute(this.UserPosition, this.SelectedPin.Position);
                    }
                });
            }
        }

        public Command SetCalculatedRouteCommand {
            get {
                return new Command(() => {
                    if (this._routeProcessing) return;
                    if (this.UserPosition != default(Position) && this.SelectedPin != null) {
                        this.RouteFrom = this.UserPosition;
                        this.RouteTo = this.SelectedPin.Position;
                        this._routeProcessing = true;
                        this.MapFunctions.SetCalculatedRoute(this.UserPosition, this.SelectedPin.Position);
                    }
                });
            }
        }

        public ICommand DeleteRouteCommand {
            get {
                return new Command(() => {
                    this.MapFunctions.DeleteRoute();
                    this.IsRouteActive = false;
                    this._routeProcessing = false; //на всякий случай
                });
            }
        }

        public ICommand RouteInitializingSucceedCommand {
            get {
                return new Command(() => {
                    this.IsRouteActive = true;
                    this._routeProcessing = false;
                });
            }
        }

        public ICommand RouteInitializingFailedCommand {
            get {
                return new Command<CustomPin>(pin => {
                    this._routeProcessing = false;
                    this.IsRouteActive = false;
                    App.ToastNotificator.Show("Ошибка построения маршрута");
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
            get => new Command(async () => {
                var searchPage = new SearchPage(await App.Database.GetItems<Person>(), this.SearchText);
                searchPage.ItemSelected += OnSearchItemSelected;
                searchPage.SearchTextChanged += OnSearchChanged;
 
                this.SelectedPin = null;
                await App.GlobalPage.Push(searchPage);
            });

        }

        public ICommand ResetCommand {
            get {
                return new Command(() => {
                    this.SearchText = string.Empty;
                });
            }
        }

        #endregion

        #region Event Handlers

        private void OnSearchChanged(object sender, string e) {
            this.SearchText = e;
        }

        private void OnSearchItemSelected(object sender, int id) {
            var pin = _customPins.FirstOrDefault(e => e.Id == id.ToString());

            if (pin != null) {
                this.SelectedPin = pin;
                this.FollowUser = false;
                MoveCameraToPosition(pin.Position);
            }
        }

        #endregion

        #endregion

        #region TTS

        #region Properties

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

        #endregion

        #region Commands

        public ICommand StartTTSOnSelectedPinCommand {
            get {
                return new Command(async () => {
                    if (this._ttsStarted) return;
                    this._ttsStarted = true;
                    var person = await App.Database.GetById<Person>(int.Parse(this.SelectedPin.Id));
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

        #endregion

        #region Event listeners

        private void TextToSpeech_OnStart(object sender, string e) {
            _ttsStarted = false;
            CurrentPlayed = int.Parse(e);
        }

        private void TextToSpeech_OnEnd(object sender, string e) {
            _ttsStarted = false;
            CurrentPlayed = null;
        }

        #endregion

        private bool _ttsStarted = false;

        private void SpeakPersonText(Person person) {
            App.TextToSpeech.Speak(person.Text, person.Id);
        }

        private void StopSpeakPersonText() {
            App.TextToSpeech.Stop();
        }


        #endregion

        #region Database

        private async void InitPinsFromMemory() {
            var list = await App.Database.GetItems<Person>();

            if (list.Count() > 0) {
                var buf = new ObservableCollection<CustomPin>();

                foreach (var item in list) {
                    var pin = CreatePin(item);
                    buf.Add(pin);
                }

                CustomPins = buf;
            }
        }

        private void Database_LoadSuccess() {
            InitPinsFromMemory();
        }

        #endregion

        #region SideMenu

        public ICommand OpenMenuCommand {
            get {
                return new Command(() => {
                    if (Application.Current.MainPage is MainPage) {
                        App.SetShowMenu(true);
                    }
                });
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }


}
