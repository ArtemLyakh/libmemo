using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo {
    public class CustomMap : Map, IMapFunctions {
        public CustomMap() : base() { }


        #region MapFunctions
        public INativeMapFunction MapFunctions {
            get { return (INativeMapFunction)this.GetValue(MapFunctionsProperty); }
            private set { this.SetValue(MapFunctionsProperty, value); }
        }
        public static readonly BindableProperty MapFunctionsProperty = BindableProperty.Create(
            nameof(MapFunctions),
            typeof(INativeMapFunction),
            typeof(CustomMap),
            default(INativeMapFunction),
            defaultBindingMode: BindingMode.OneWayToSource);

        void IMapFunctions.SetRenderer(INativeMapFunction renderer) {
            this.MapFunctions = renderer;
        }
        #endregion


        protected override void OnPropertyChanged(string propertyName = null) {
            base.OnPropertyChanged(propertyName);
        }

        #region ICustomMapFunction Implementations

        #region CameraPositionChanged
        public class CameraPositionChangeEventArgs : EventArgs {
            public Position Position { get; set; }
            public float Zoom { get; set; }
        }

        public event EventHandler<CameraPositionChangeEventArgs> CameraPositionChanged;

        void IMapFunctions.RaiseCameraPositionChange(Position position, float zoom) {
            var args = new CameraPositionChangeEventArgs() {
                Position = position,
                Zoom = zoom
            };
            CameraPositionChanged?.Invoke(this, args);


            var commandArgs = new Tuple<Position, float>(position, zoom);
            if (CameraPositionChangedCommand != null && CameraPositionChangedCommand.CanExecute(commandArgs)) {
                CameraPositionChangedCommand.Execute(commandArgs);
            }
        }
        #endregion

        #region InfoWindowClicked
        public class InfoWindowClickedEventArgs : EventArgs {
            public CustomPin Pin { get; set; }
        }

        public event EventHandler<InfoWindowClickedEventArgs> InfoWindowClicked;

        void IMapFunctions.RaiseInfoWindowClick(CustomPin pin) {
            var args = new InfoWindowClickedEventArgs() { Pin = pin };
            InfoWindowClicked?.Invoke(this, args);

            if (InfoWindowClickedCommand != null && InfoWindowClickedCommand.CanExecute(pin)) {
                InfoWindowClickedCommand.Execute(pin);
            }
        }
        #endregion

        #region UserPositionChanged
        public class UserPositionChangeEventArgs {
            public Position Position { get; set; }
        }

        public event EventHandler<UserPositionChangeEventArgs> UserPositionChanged;

        void IMapFunctions.RaiseUserLocationChange(Position position) {
            var args = new UserPositionChangeEventArgs() {
                Position = position
            };
            UserPositionChanged?.Invoke(this, args);

            if (UserPositionChangedCommand != null && UserPositionChangedCommand.CanExecute(position)) {
                UserPositionChangedCommand.Execute(position);
            }
        }
        #endregion

        #region RouteInitializingSucceed
        public event EventHandler RouteInitializingSucceed;

        void IMapFunctions.RaiseRouteInitializingSucceed() {
            RouteInitializingSucceed?.Invoke(this, null);

            if (RouteInitializingSucceedCommand != null && RouteInitializingSucceedCommand.CanExecute(null)) {
                RouteInitializingSucceedCommand.Execute(null);
            }
        }
        #endregion

        #region RouteInitializingFailed
        public event EventHandler RouteInitializingFailed;

        void IMapFunctions.RaiseRouteInitializingFailed() {
            RouteInitializingFailed?.Invoke(this, null);

            if (RouteInitializingFailedCommand != null && RouteInitializingFailedCommand.CanExecute(null)) {
                RouteInitializingFailedCommand.Execute(null);
            }
        }
        #endregion

        #endregion

        #region Properties

        #region Camera
        #region MapCenter
        public Position MapCenter {
            get { return (Position)this.GetValue(MapCenterProperty); }
            set { this.SetValue(MapCenterProperty, value); }
        }
        public static readonly BindableProperty MapCenterProperty = BindableProperty.Create(
            nameof(MapCenter),
            typeof(Position),
            typeof(CustomMap),
            default(Position),
            defaultBindingMode: BindingMode.TwoWay);
        #endregion
        #region Zoom
        public float Zoom {
            get { return (float)this.GetValue(ZoomProperty); }
            set { this.SetValue(ZoomProperty, value); }
        }
        public static readonly BindableProperty ZoomProperty = BindableProperty.Create(
            nameof(Zoom),
            typeof(float),
            typeof(CustomMap),
            default(float),
            defaultBindingMode: BindingMode.TwoWay);
        #endregion
        #region IsCameraAnimated
        public bool IsCameraAnimated {
            get { return (bool)this.GetValue(IsCameraAnimatedProperty); }
            set { this.SetValue(IsCameraAnimatedProperty, value); }
        }
        public static readonly BindableProperty IsCameraAnimatedProperty = BindableProperty.Create(
            nameof(IsCameraAnimated),
            typeof(bool),
            typeof(CustomMap),
            false);
        #endregion
        #endregion

        #region Gestures
        #region IsRotateGesturesEnabled
        public bool IsRotateGesturesEnabled {
            get { return (bool)this.GetValue(IsRotateGesturesEnabledProperty); }
            set { this.SetValue(IsRotateGesturesEnabledProperty, value); }
        }
        public static readonly BindableProperty IsRotateGesturesEnabledProperty = BindableProperty.Create(
            nameof(IsRotateGesturesEnabled),
            typeof(bool),
            typeof(CustomMap),
            true);
        #endregion
        #region IsScrollGesturesEnabled
        public bool IsScrollGesturesEnabled {
            get { return (bool)this.GetValue(IsScrollGesturesEnabledProperty); }
            set { this.SetValue(IsScrollGesturesEnabledProperty, value); }
        }
        public static readonly BindableProperty IsScrollGesturesEnabledProperty = BindableProperty.Create(
            nameof(IsScrollGesturesEnabled),
            typeof(bool),
            typeof(CustomMap),
            true);
        #endregion
        #region IsTiltGesturesEnabled
        public bool IsTiltGesturesEnabled {
            get { return (bool)this.GetValue(IsTiltGesturesEnabledProperty); }
            set { this.SetValue(IsTiltGesturesEnabledProperty, value); }
        }
        public static readonly BindableProperty IsTiltGesturesEnabledProperty = BindableProperty.Create(
            nameof(IsTiltGesturesEnabled),
            typeof(bool),
            typeof(CustomMap),
            true);
        #endregion
        #region IsZoomGesturesEnabled 
        public bool IsZoomGesturesEnabled {
            get { return (bool)this.GetValue(IsZoomGesturesEnabledProperty); }
            set { this.SetValue(IsZoomGesturesEnabledProperty, value); }
        }
        public static readonly BindableProperty IsZoomGesturesEnabledProperty = BindableProperty.Create(
            nameof(IsZoomGesturesEnabled),
            typeof(bool),
            typeof(CustomMap),
            true);
        #endregion
        #endregion

        #region SelectedPin
        public CustomPin SelectedPin {
            get { return (CustomPin)this.GetValue(SelectedPinProperty); }
            set { this.SetValue(SelectedPinProperty, value); }
        }
        public static readonly BindableProperty SelectedPinProperty = BindableProperty.Create(
            nameof(SelectedPin),
            typeof(CustomPin),
            typeof(CustomMap),
            default(CustomPin),
            defaultBindingMode: BindingMode.TwoWay);
        #endregion

        #region CustomPins
        public ObservableCollection<CustomPin> CustomPins {
            get { return (ObservableCollection<CustomPin>)this.GetValue(CustomPinsProperty); }
            set { this.SetValue(CustomPinsProperty, value); }
        }
        public static readonly BindableProperty CustomPinsProperty = BindableProperty.Create(
            nameof(CustomPins),
            typeof(ObservableCollection<CustomPin>),
            typeof(CustomMap));
        #endregion

        #region RouteTo
        public CustomPin RouteTo {
            get { return (CustomPin)this.GetValue(RouteToProperty); }
            set { this.SetValue(RouteToProperty, value); }
        }
        public static readonly BindableProperty RouteToProperty = BindableProperty.Create(
            nameof(RouteTo),
            typeof(CustomPin),
            typeof(CustomMap),
            default(CustomPin),
            defaultBindingMode: BindingMode.OneWay);
        #endregion

        #region MyLocationEnabled
        public bool MyLocationEnabled {
            get { return (bool)this.GetValue(MyLocationEnabledProperty); }
            set { this.SetValue(MyLocationEnabledProperty, value); }
        }
        public static readonly BindableProperty MyLocationEnabledProperty = BindableProperty.Create(
            nameof(MyLocationEnabled),
            typeof(bool),
            typeof(CustomMap),
            true);
        #endregion

        #endregion

        #region Commands

        #region InfoWindowClicked
        public ICommand InfoWindowClickedCommand {
            get { return (ICommand)this.GetValue(InfoWindowClickedCommandProperty); }
            set { this.SetValue(InfoWindowClickedCommandProperty, value); }
        }
        public static readonly BindableProperty InfoWindowClickedCommandProperty = BindableProperty.Create(
            nameof(InfoWindowClickedCommand),
            typeof(Command<CustomPin>),
            typeof(CustomMap));
        #endregion

        #region CameraPositionChanged
        public ICommand CameraPositionChangedCommand {
            get { return (ICommand)this.GetValue(CameraPositionChangedCommandProperty); }
            set { this.SetValue(CameraPositionChangedCommandProperty, value); }
        }
        public static readonly BindableProperty CameraPositionChangedCommandProperty = BindableProperty.Create(
            nameof(CameraPositionChangedCommand),
            typeof(Command<Tuple<Position, float>>),
            typeof(CustomMap));
        #endregion

        #region UserPositionChanged
        public ICommand UserPositionChangedCommand {
            get { return (ICommand)this.GetValue(UserPositionChangedCommandProperty); }
            set { this.SetValue(UserPositionChangedCommandProperty, value); }
        }
        public static readonly BindableProperty UserPositionChangedCommandProperty = BindableProperty.Create(
            nameof(UserPositionChangedCommand),
            typeof(Command<Position>),
            typeof(CustomMap));
        #endregion

        #region RouteInitializingSucceed
        public ICommand RouteInitializingSucceedCommand {
            get { return (ICommand)this.GetValue(RouteInitializingSucceedCommandProperty); }
            set { this.SetValue(RouteInitializingSucceedCommandProperty, value); }
        }
        public static readonly BindableProperty RouteInitializingSucceedCommandProperty = BindableProperty.Create(
            nameof(RouteInitializingSucceedCommand),
            typeof(Command),
            typeof(CustomMap));
        #endregion

        #region RouteInitializingFailed
        public ICommand RouteInitializingFailedCommand {
            get { return (ICommand)this.GetValue(RouteInitializingFailedCommandProperty); }
            set { this.SetValue(RouteInitializingFailedCommandProperty, value); }
        }
        public static readonly BindableProperty RouteInitializingFailedCommandProperty = BindableProperty.Create(
            nameof(RouteInitializingFailedCommand),
            typeof(Command),
            typeof(CustomMap));
        #endregion

        #endregion
    }
}
