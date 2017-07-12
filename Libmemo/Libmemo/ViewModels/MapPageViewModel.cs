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

            GetGPSPermission();

            Zoom = DEFAULT_ZOOM;
            MapCenter = new Position(DEFAULT_LATITUDE, DEFAULT_LONGITUDE);
        }

        public ICommand SetupPins => new Command(() => InitPinsFromMemory());

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

        private CustomPin _selectedPin = null;
        public CustomPin SelectedPin {
            get { return _selectedPin; }
            set {
                if (_selectedPin != value) {
                    _selectedPin = value;
                    OnPropertyChanged(nameof(SelectedPin));
                    OnPropertyChanged(nameof(IsShowHideAnotherButton));
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

        public ICommand InfoWindowClickedCommand => new Command<CustomPin>(async (CustomPin pin) => {
            if (int.TryParse(pin.Id, out int id)) {
                var page = new DetailPage(id);
                await App.GlobalPage.Push(page);
            }
        });

        private CustomPin CreatePin(Person person) => new CustomPin() {
            Id = person.Id.ToString(),
            PinImage = string.IsNullOrWhiteSpace(person.Text) ? PinImage.Default : PinImage.Speaker,
            Position = new Position(person.Latitude, person.Longitude),
            Title = person.FIO,
            Text = person.DateBirth.HasValue && person.DateDeath.HasValue ? $"{person.DateBirth.Value.Date.ToString("dd.MM.yyyy")}\u2014{person.DateDeath.Value.ToString("dd.MM.yyyy")}" : "",
            Visible = true,
            IconUri = person.IconUrl
        };

        #endregion

        #region Camera

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

        private void MoveCameraToUserPosition() {
            MoveCameraToPosition(this.UserPosition);
        }

        private void MoveCameraToPosition(Position position) {
            this.MapCenter = position;
        }

        #endregion

        #region GPS

        private Position UserPosition { get; set; }

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

        private bool firstCamera = true;
        public ICommand UserPositionChangedCommand => new Command<Position>((Position position) => {
            this.UserPosition = position;

            if (this.FollowUser) {
                if (firstCamera) {
                    this.IsCameraAnimated = true;
                    firstCamera = false;
                }
                MoveCameraToUserPosition();
            }
        });


        public ICommand FollowUserToogleCommand => new Command(() => {
            if (FollowUser) {
                FollowUser = false;
            } else {
                FollowUser = true;
                MoveCameraToUserPosition();
            }
        });


        #endregion

        #region Routes

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

        private Position? RouteFrom { get; set; }
        private Position? RouteTo { get; set; }

        private bool _routeProcessing = false;

        public ICommand SetLinearRouteCommand => new Command(() => {
            if (this._routeProcessing) return;
            if (this.UserPosition != default(Position) && this.SelectedPin != null) {
                this.RouteFrom = this.UserPosition;
                this.RouteTo = this.SelectedPin.Position;
                this._routeProcessing = true;
                this.MapFunctions.SetLinearRoute(this.UserPosition, this.SelectedPin.Position);
            }
        });


        public ICommand SetCalculatedRouteCommand => new Command(() => {
            if (this._routeProcessing) return;
            if (this.UserPosition != default(Position) && this.SelectedPin != null) {
                this.RouteFrom = this.UserPosition;
                this.RouteTo = this.SelectedPin.Position;
                this._routeProcessing = true;
                this.MapFunctions.SetCalculatedRoute(this.UserPosition, this.SelectedPin.Position);
            }
        });


        public ICommand DeleteRouteCommand => new Command(() => {
            this.MapFunctions.DeleteRoute();
            this.IsRouteActive = false;
            this._routeProcessing = false; //на всякий случай
        });


        public ICommand RouteInitializingSucceedCommand => new Command(() => {
            this.IsRouteActive = true;
            this._routeProcessing = false;
        });


        public ICommand RouteInitializingFailedCommand => new Command<CustomPin>(pin => {
            this._routeProcessing = false;
            this.IsRouteActive = false;
            App.ToastNotificator.Show("Ошибка построения маршрута");
        });


        #endregion

        #region Search

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

        public ICommand SearchCommand => new Command(async () => {
            var page = new MapSearchPage(this.SearchText);

            page.SearchTextChanged += (sender, text) => this.SearchText = text;
            page.ItemSelected += async (sender, person) => {
                await App.GlobalPage.Pop();
                var pin = _customPins.FirstOrDefault(e => e.Id == person.Id.ToString());
                if (pin != null) {
                    ShowAllPins();
                    this.SelectedPin = pin;
                    this.FollowUser = false;
                    MoveCameraToPosition(pin.Position);
                }
            };

            await App.GlobalPage.Push(page);
        });



        #endregion

        #region TTS

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

        public ICommand StartTTSOnSelectedPinCommand {
            get => new Command(async () => {
                if (this._ttsStarted) return;
                this._ttsStarted = true;
                var person = await App.Database.GetById(int.Parse(this.SelectedPin.Id));
                SpeakPersonText(person);
            });
        }
        public ICommand StopTTSCommand {
            get => new Command(() => StopSpeakPersonText());        
        }

        private void TextToSpeech_OnStart(object sender, string e) {
            _ttsStarted = false;
            CurrentPlayed = int.Parse(e);
        }

        private void TextToSpeech_OnEnd(object sender, string e) {
            _ttsStarted = false;
            CurrentPlayed = null;
        }

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
            var list = await App.Database.GetList(PersonType.Dead);

            CustomPins.Clear();
            foreach (var item in list) {
                var pin = CreatePin(item);
                CustomPins.Add(pin);
            }      
        }

        private void Database_LoadSuccess() {
            InitPinsFromMemory();
        }

        #endregion

        #region SideMenu

        public ICommand OpenMenuCommand => new Command(() => App.SetShowMenu(true));

        #endregion





        private MapType _mapType = MapType.Street;
        public MapType MapType {
            get => this._mapType;
            set {
                if (this._mapType != value) {
                    this._mapType = value;
                    this.OnPropertyChanged(nameof(MapType));
                }
            }
        }

        public ICommand SpaceMapCommand => new Command(() => this.MapType = MapType.Hybrid);

        public ICommand StreetMapCommand => new Command(() => this.MapType = MapType.Street);




        private void HideAllPinsExcept(string id) {
            IsShowOnlySelected = true;
            foreach (var pin in CustomPins) {
                if (pin.Id == id) pin.Visible = true;
                else pin.Visible = false;
            }
        }
        private void ShowAllPins() {
            IsShowOnlySelected = false;
            foreach (var pin in CustomPins) {
                pin.Visible = true;
            }
        }

        private bool _isShowOnlySelected = false;
        public bool IsShowOnlySelected {
            get => _isShowOnlySelected;
            set {
                if (_isShowOnlySelected != value) {
                    _isShowOnlySelected = value;
                    OnPropertyChanged(nameof(IsShowOnlySelected));
                    OnPropertyChanged(nameof(IsShowHideAnotherButton));
                }
            }
        }

        public bool IsShowHideAnotherButton {
            get => SelectedPin != null && !IsShowOnlySelected;
        }

        public ICommand HidePinsCommand => new Command(() => {           
            if (SelectedPin != null) HideAllPinsExcept(SelectedPin.Id);
            else ShowAllPins();
        });


        public ICommand ShowPinsCommand => new Command(() => {
            ShowAllPins();
        });



        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }


}
