using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Libmemo.Pages.Map
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Map : ContentPage
    {
        private ViewModel Model {
            get => (ViewModel)BindingContext;
            set => BindingContext = value;
        }

        public Map(List<Models.DeadPerson> persons)
        {
            BindingContext = new ViewModel(persons);
            InitializeComponent();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            Model.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Model.OnDisappearing();
        }


        public class ViewModel : BaseViewModel
        {
            private Dictionary<int, Models.DeadPerson> Data;


            public ViewModel(List<Models.DeadPerson> persons) : base()
            {
                Data = persons.ToDictionary(i => i.Id);

                var pins = new ObservableCollection<CustomPin>();
                foreach (var person in persons) {
                    pins.Add(CreatePin(person));
                }

                CustomPins = pins;
            }

            public override void OnAppearing()
            {
                base.OnAppearing();

                MyLocationEnabled = true;

                //TTS listeners
                App.TextToSpeech.OnStart += TextToSpeech_OnStart;
                App.TextToSpeech.OnEnd += TextToSpeech_OnEnd;
            }

            public override void OnDisappearing()
            {
                base.OnDisappearing();

                MyLocationEnabled = false;

                //TTS listeners
                App.TextToSpeech.OnStart -= TextToSpeech_OnStart;
                App.TextToSpeech.OnEnd -= TextToSpeech_OnEnd;
            }


            private string _title = "Карта";
            public string Title {
                get => _title;
                set {
                    if (value != _title) {
                        _title = value;
                        OnPropertyChanged(nameof(Title));
                    }
                }
            }
            private void SetDefaultTitle()
            {
                Title = "Карта";
            }


            public INativeMapFunction MapFunctions { get; set; }

            #region Pins

            private CustomPin _selectedPin = null;
            public CustomPin SelectedPin {
                get => _selectedPin;
                set {
                    if (_selectedPin != value) {
                        _selectedPin = value;
                        OnPropertyChanged(nameof(SelectedPin));
                        OnPropertyChanged(nameof(IsShowHideAnotherButton));
                    }
                }
            }

            ObservableCollection<CustomPin> _customPins;
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
                await App.GlobalPage.Push(new Pages.Detail(int.Parse(pin.Id)));
            });

            private static CustomPin CreatePin(Models.DeadPerson person) => new CustomPin() {
                Id = person.Id.ToString(),
                PinImage = string.IsNullOrWhiteSpace(person.Text) ? PinImage.Default : PinImage.Speaker,
                Position = new Position(person.Latitude, person.Longitude),
                Title = person.FIO,
                Text = person.DateBirth.HasValue && person.DateDeath.HasValue ? $"{person.DateBirth.Value.Date.ToString("dd.MM.yyyy")}\u2014{person.DateDeath.Value.ToString("dd.MM.yyyy")}" : "",
                Visible = true,
                IconUri = person.Icon
            };

            #endregion

            #region Camera

            private const double DEFAULT_LATITUDE = 47.23135;
            private const double DEFAULT_LONGITUDE = 39.72328;
            private const float DEFAULT_ZOOM = 18;

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

            private void MoveCameraToUserPosition()
            {
                MoveCameraToPosition(this.UserPosition);
            }

            private void MoveCameraToPosition(Position position)
            {
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

            private event EventHandler<Position> UserPositionChanged;

            private bool firstCamera = true;
            public ICommand UserPositionChangedCommand => new Command<Position>(position => {
                this.UserPosition = position;

                if (this.FollowUser) {
                    if (firstCamera) {
                        this.IsCameraAnimated = true;
                        firstCamera = false;
                    }
                    MoveCameraToUserPosition();
                }

                UserPositionChanged?.Invoke(this, position);
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

            public enum RouteType
            {
                None, Linear, Calculated
            }

            private RouteType _currentRoute = RouteType.None;
            private RouteType CurrentRoute {
                get => _currentRoute;
                set {
                    if (_currentRoute != value) {
                        _currentRoute = value;
                        OnPropertyChanged(nameof(CurrentRoute));
                        OnPropertyChanged(nameof(IsRouteActive));
                    }
                }
            }

            public bool IsRouteActive => CurrentRoute != RouteType.None;

            private Position? RouteFrom { get; set; }
            private Position? RouteTo { get; set; }
            private CustomPin RouteToPin { get; set; }

            private bool _routeProcessing = false;

            public ICommand SetLinearRouteCommand => new Command(async () => {
                if (this._routeProcessing) return;

                DeleteRouteCommand.Execute(null);

                if (this.UserPosition != default(Position) && this.SelectedPin != null) {
                    RouteToPin = SelectedPin;

                    var from = this.UserPosition;
                    var to = this.SelectedPin.Position;

                    try {
                        _routeProcessing = true;
                        await this.MapFunctions.SetLinearRouteAsync(from, to);
                    } catch {
                        App.ToastNotificator.Show("Ошибка построения маршрута");
                        return;
                    } finally {
                        _routeProcessing = false;
                    }

                    this.CurrentRoute = RouteType.Linear;
                    this.RouteFrom = from;
                    this.RouteTo = to;

                    var distance = CalculateDistance(RouteFrom.Value, RouteTo.Value);
                    distance = Math.Round(distance);
                    this.Title = $"~ {distance.ToString("N0")} м";

                    UserPositionChanged -= OnUserPositionChangedUpdateLinearRoute;
                    UserPositionChanged += OnUserPositionChangedUpdateLinearRoute;

                    UserPositionChanged += OnUserPositionCloseToRouteEnd;
                }
            });
            private void OnUserPositionCloseToRouteEnd(object sender, Position position)
            {
                if (!RouteTo.HasValue || RouteToPin == null || CurrentRoute != RouteType.Linear) return;
                if (CalculateDistance(position, RouteTo.Value) <= 15) {
                    SelectedPin = RouteToPin;
                    UserPositionChanged -= OnUserPositionCloseToRouteEnd;
                }
            }
            private void OnUserPositionChangedUpdateLinearRoute(object sender, Position position)
            {
                UpdateLinearRoute.Execute(position);
            }
            public ICommand UpdateLinearRoute => new Command<Position>(async position => {
                if (_routeProcessing || CurrentRoute != RouteType.Linear || !RouteTo.HasValue) return;

                var from = position;
                var to = RouteTo.Value;

                try {
                    _routeProcessing = true;
                    await this.MapFunctions.SetLinearRouteAsync(from, to);
                } catch {
                    App.ToastNotificator.Show("Ошибка построения маршрута");
                    UserPositionChanged -= OnUserPositionChangedUpdateLinearRoute;
                    return;
                } finally {
                    _routeProcessing = false;
                }

                this.RouteFrom = from;

                var distance = CalculateDistance(RouteFrom.Value, RouteTo.Value);
                distance = Math.Round(distance);
                this.Title = $"~ {distance.ToString("N0")} м";
            });

            private double CalculateDistance(Position A, Position B)
            {
                double d1 = A.Latitude * 0.017453292519943295;
                double d2 = A.Longitude * 0.017453292519943295;
                double d3 = B.Latitude * 0.017453292519943295;
                double d4 = B.Longitude * 0.017453292519943295;
                double d5 = d4 - d2;
                double d6 = d3 - d1;
                double d7 = Math.Pow(Math.Sin(d6 / 2.0), 2.0) + ((Math.Cos(d1) * Math.Cos(d3)) * Math.Pow(Math.Sin(d5 / 2.0), 2.0));
                double d8 = 2.0 * Math.Atan2(Math.Sqrt(d7), Math.Sqrt(1.0 - d7));
                return (6376500.0 * d8);
            }

            public ICommand SetCalculatedRouteCommand => new Command(async () => {
                if (this._routeProcessing) return;

                DeleteRouteCommand.Execute(null);

                if (this.UserPosition != default(Position) && this.SelectedPin != null) {
                    RouteToPin = SelectedPin;

                    var from = this.UserPosition;
                    var to = this.SelectedPin.Position;

                    try {
                        _routeProcessing = true;
                        await this.MapFunctions.SetCalculatedRouteAsync(from, to);
                    } catch {
                        App.ToastNotificator.Show("Ошибка построения маршрута");
                        return;
                    } finally {
                        _routeProcessing = false;
                    }

                    this.CurrentRoute = RouteType.Calculated;
                    this.RouteFrom = from;
                    this.RouteTo = to;
                }
            });


            public ICommand DeleteRouteCommand => new Command(() => {
                this.MapFunctions.DeleteRoute();
                this.CurrentRoute = RouteType.None;

                RouteToPin = null;
                UserPositionChanged -= OnUserPositionCloseToRouteEnd;
                UserPositionChanged -= OnUserPositionChangedUpdateLinearRoute;
                SetDefaultTitle();
            });

            #endregion


            public ICommand SearchCommand => new Command(async () => {
                await App.GlobalPage.Push(new Pages.Map.Search(
                    Data.Select(i => i.Value).ToList(),
                    async id => {
                        await App.GlobalPage.Pop();
                        var pin = CustomPins.First(i => i.Id == id.ToString());
                        ShowAllPins();
                        this.SelectedPin = pin;
                        this.FollowUser = false;
                        MoveCameraToPosition(pin.Position);
                    }
                ));
            });


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

            public ICommand StartTTSOnSelectedPinCommand => new Command(() => {
                if (this._ttsStarted) return;
                this._ttsStarted = true;
                var person = Data[int.Parse(this.SelectedPin.Id)];
                SpeakPersonText(person);
            });
            public ICommand StopTTSCommand => new Command(() => StopSpeakPersonText());


            private void TextToSpeech_OnStart(object sender, string e)
            {
                _ttsStarted = false;
                CurrentPlayed = int.Parse(e);
            }

            private void TextToSpeech_OnEnd(object sender, string e)
            {
                _ttsStarted = false;
                CurrentPlayed = null;
            }

            private bool _ttsStarted = false;

            private void SpeakPersonText(Models.DeadPerson person)
            {
                App.TextToSpeech.Speak(person.Text, person.Id);
            }

            private void StopSpeakPersonText()
            {
                App.TextToSpeech.Stop();
            }

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




            private void HideAllPinsExcept(string id)
            {
                IsShowOnlySelected = true;
                foreach (var pin in CustomPins) {
                    if (pin.Id == id) pin.Visible = true;
                    else pin.Visible = false;
                }
            }
            private void ShowAllPins()
            {
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


        }
    }
}