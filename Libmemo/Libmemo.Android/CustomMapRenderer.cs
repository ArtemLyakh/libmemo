﻿using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;

using Android.Content;
using Android.Widget;

using Android.Gms.Maps;
using Android.Gms.Maps.Model;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Platform.Android;
using System.Threading.Tasks;


[assembly: ExportRenderer(typeof(Libmemo.CustomMap), typeof(Libmemo.Droid.CustomMapRenderer))]
namespace Libmemo.Droid {
    class CustomMapRenderer : MapRenderer, INativeMapFunction, GoogleMap.IInfoWindowAdapter, IOnMapReadyCallback {

        #region INativeMapFunction

        void INativeMapFunction.DeleteRoute() {
            DeleteExistingRoute();
        }

		async Task INativeMapFunction.SetLinearRouteAsync(Position from, Position to)
		{
			DeleteExistingRoute();
            await AddLinearRoute(from, to);
		}

		async Task INativeMapFunction.SetCalculatedRouteAsync(Position from, Position to)
		{
			DeleteExistingRoute();
            await AddCalculatedRoute(from, to);
		}

        #endregion

        #region Aliases
        private CustomMap FormsMap { get { return this.Element as CustomMap; } }
        private IMapFunctions MapFunctions { get { return this.Element as IMapFunctions; } }
        #endregion

        GoogleMap _googleMap;
        LatLng _userLocation = null;

        #region FormMap properties

        MapType _mapType;

        Position _mapCenter;

        float _zoom;
        bool _isCameraAnimated;

        bool _isRotateGesturesEnabled;
        bool _isScrollGesturesEnabled;
        bool _isTiltGesturesEnabled;
        bool _isZoomGesturesEnabled;

        bool _isShowInfoWindow;

        CustomPin _selectedPin = null;

        ObservableCollection<CustomPin> _customPins;
        Dictionary<CustomPin, Marker> _customPinsBindings = new Dictionary<CustomPin, Marker>();

        Polyline _route = null;
        List<LatLng> _routePoints = null;

        bool _myLocationEnabled = false;

        #endregion

        #region Engine functions

        protected override void OnElementChanged(ElementChangedEventArgs<Map> e) {
            base.OnElementChanged(e);

            if (e.OldElement != null) {
                e.OldElement.PropertyChanged -= FormsMapPropertyChanged;
                UnregisterCustomPinsChangingEvent((CustomMap)e.OldElement);

                RemoveEventHandlers();
            }

            if (e.NewElement != null) {
                SetFormsMapProperties((CustomMap)e.NewElement);

                ((MapView)Control).GetMapAsync(this);

                this.MapFunctions.SetRenderer(this);

                this.FormsMap.PropertyChanged += FormsMapPropertyChanged;
                RegisterCustomPinsChangingEvent();
            }
        }

        public void OnMapReady(GoogleMap googleMap) {
            _googleMap = googleMap;

            AddEventHandlers();

            _googleMap.SetInfoWindowAdapter(this);
        }

        bool isDrawn;
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.OnElementPropertyChanged(sender, e);

            if (_googleMap != null && !isDrawn) {
                foreach (var bind in _customPinsBindings) {
                    bind.Value.Remove();
                }
                _customPinsBindings = new Dictionary<CustomPin, Marker>();

                _route?.Remove();
                _route = null;

                try {
                    _googleMap.Clear();
                } catch { }
                

                FullDrawMap();

                isDrawn = true;
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b) {
            base.OnLayout(changed, l, t, r, b);

            if (changed) {
                isDrawn = false;
            }
        }

        #endregion

        #region Listen/stop to custom pin collection changing

        private void RegisterCustomPinsChangingEvent() {
            if (this.FormsMap?.CustomPins is System.Collections.ObjectModel.ObservableCollection<CustomPin>) {
                (this.FormsMap?.CustomPins as System.Collections.ObjectModel.ObservableCollection<CustomPin>).CollectionChanged += CustomPinsCollectionChanged;
            }
        }

        private void UnregisterCustomPinsChangingEvent(CustomMap oldMap) {
            if (oldMap?.CustomPins is System.Collections.ObjectModel.ObservableCollection<CustomPin>) {
                (oldMap?.CustomPins as System.Collections.ObjectModel.ObservableCollection<CustomPin>).CollectionChanged -= CustomPinsCollectionChanged;
            }
        }

        #endregion

        #region Binding listeners on custom pins collection changed

        private void CustomPinsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.OldItems != null) {
                foreach (var i in e.OldItems) {
                    if (i is CustomPin) {
                        (i as CustomPin).PropertyChanged -= CustomPin_PropertyChanged;
                        DeletePin(i as CustomPin);
                    }
                }
            }

            if (e.NewItems != null) {
                foreach (var i in e.NewItems) {
                    if (i is CustomPin) {
                        (i as CustomPin).PropertyChanged += CustomPin_PropertyChanged;
                        AddPin(i as CustomPin);
                    }
                }
            }
        }

        #endregion



        #region Utils

        private void SetFormProperties(Action action) {
            if (this.FormsMap == null) return;
            this.FormsMap.PropertyChanged -= FormsMapPropertyChanged;
            action.Invoke();
            this.FormsMap.PropertyChanged += FormsMapPropertyChanged;
        }



        private void AddPin(CustomPin pin) {
            var marker = new MarkerOptions();

            marker.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
            marker.SetTitle(pin.Title);
            marker.SetSnippet(pin.Text);

            if (pin.PinImage == PinImage.Default) {
                marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.default_pin));
            } else if (pin.PinImage == PinImage.Speaker) {
                marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.speaker_pin));
            } else {
                throw new NotImplementedException();
            }

            if (_googleMap != null) {
                if (_customPinsBindings.ContainsKey(pin) && _customPinsBindings[pin] != null) 
                    _customPinsBindings[pin].Remove();
                _customPinsBindings[pin] = _googleMap.AddMarker(marker);
            }
        }

        private void DeletePin(CustomPin pin) {
            var marker = _customPinsBindings[pin];
            _customPinsBindings.Remove(pin);

            marker.Remove();
        }

        private void UpdatePin(CustomPin pin, Marker m, string propName) {
            if (propName == nameof(CustomPin.PinImage)) {
                if (pin.PinImage == PinImage.Default) {
                    m.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.default_pin));
                } else if (pin.PinImage == PinImage.Speaker) {
                    m.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.speaker_pin));
                } else {
                    throw new NotImplementedException();
                }
            } else if (propName == nameof(CustomPin.Position)) {
                m.Position = new LatLng(pin.Position.Latitude, pin.Position.Longitude);
            } else if (propName == nameof(CustomPin.Visible)) {
                if (m != null) 
                m.Visible = pin.Visible;
            } else {
                throw new NotImplementedException();
            }
        }



        private void ChangeCamera(Position? position = null, float? zoom = null) {
            double newLat = position.HasValue ? position.Value.Latitude : _googleMap.CameraPosition.Target.Latitude;
            double newLon = position.HasValue ? position.Value.Longitude : _googleMap.CameraPosition.Target.Longitude;
            float newZoom = zoom.HasValue ? zoom.Value : _googleMap.CameraPosition.Zoom;

            var cameraUpdate = CameraUpdateFactory.NewLatLngZoom(new LatLng(newLat, newLon), newZoom);

            if (this._isCameraAnimated) {
                this._googleMap.AnimateCamera(cameraUpdate);
            } else {
                this._googleMap.MoveCamera(cameraUpdate);
            }
        }

        private void MoveCamera() {
            ChangeCamera(_mapCenter, _zoom);
        }



        private PolylineOptions GetPolylineOptions() {
            var line = new PolylineOptions();
            line.Clickable(false);
            line.Visible(true);
            line.InvokeColor(Color.Red.ToAndroid());
            line.InvokeWidth(5);
            return line;
        }

        private void DeleteExistingRoute() {
            if (this._route != null) {
                this._route.Remove();
                this._route = null;
                this._routePoints = null;
            }
        }

        private async Task AddLinearRoute(Position from, Position to) {
            await SetLinearRoute(from, to);
            this._route = DrawRoute();
            if (_route == null) throw new Exception("Route exception");

            //if (this._route == null) {
            //    this.MapFunctions?.RaiseRouteInitializingFailed();
            //} else {
            //    this.MapFunctions?.RaiseRouteInitializingSucceed();
            //}
        }
        private async Task AddCalculatedRoute(Position from, Position to) {
            await DrawCalculatedRoute(from, to);
            this._route = DrawRoute();
            if (_route == null) throw new Exception("Route exception");


            //if (this._route == null) {
            //    this.MapFunctions?.RaiseRouteInitializingFailed();
            //} else {
            //    this.MapFunctions?.RaiseRouteInitializingSucceed();
            //}
        }

        private Task SetLinearRoute(Position from, Position to) => Task.Run(() => {
            _routePoints = new List<LatLng>() {
                new LatLng(from.Latitude, from.Longitude),
                new LatLng(to.Latitude, to.Longitude)
            };
        });
        private async Task DrawCalculatedRoute(Position from, Position to) {
            var routeData = await TK.CustomMap.Api.Google.GmsDirection.Instance.CalculateRoute(from, to, TK.CustomMap.Api.Google.GmsDirectionTravelMode.Walking);
            if (routeData != null && routeData.Status == TK.CustomMap.Api.Google.GmsDirectionResultStatus.Ok) {
                var r = routeData.Routes.FirstOrDefault();
                if (r != null && r.Polyline.Positions != null && r.Polyline.Positions.Any()) {
                    _routePoints = r.Polyline.Positions.Select(i => new LatLng(i.Latitude, i.Longitude)).ToList();
                }
            }
        }
        private Polyline DrawRoute() {
            if (_routePoints == null) return null;
            var route = GetPolylineOptions();
            foreach (var point in _routePoints) {
                route.Add(point);
            }
            return _googleMap.AddPolyline(route);
        }



        #endregion

        #region Engine function helpers

        private void SetFormsMapProperties(CustomMap map) {
            this._mapCenter = map.MapCenter;
            this._zoom = map.Zoom;
            this._isCameraAnimated = map.IsCameraAnimated;

            this._isRotateGesturesEnabled = map.IsRotateGesturesEnabled;
            this._isScrollGesturesEnabled = map.IsScrollGesturesEnabled;
            this._isTiltGesturesEnabled = map.IsTiltGesturesEnabled;
            this._isZoomGesturesEnabled = map.IsZoomGesturesEnabled;

            this._isShowInfoWindow = map.IsShowInfoWindow;

            this._mapType = map.MapType;

            this._customPins = map.CustomPins;
        }

        private void AddEventHandlers() {
            _googleMap.CameraChange += _googleMap_CameraChange;

            _googleMap.MarkerClick += _googleMap_MarkerClick;
            _googleMap.InfoWindowClose += _googleMap_InfoWindowClose;
            _googleMap.InfoWindowClick += _googleMap_InfoWindowClick;

            _googleMap.MyLocationChange += _googleMap_MyLocationChange;

            _googleMap.MapClick += _googleMap_MapClick;
        }


        private void RemoveEventHandlers() {
            _googleMap.CameraChange -= _googleMap_CameraChange;

            _googleMap.MarkerClick -= _googleMap_MarkerClick;
            _googleMap.InfoWindowClose -= _googleMap_InfoWindowClose;
            _googleMap.InfoWindowClick -= _googleMap_InfoWindowClick;

            _googleMap.MyLocationChange -= _googleMap_MyLocationChange;

            _googleMap.MapClick -= _googleMap_MapClick;
        }

        private void FullDrawMap() {
            _googleMap.UiSettings.MyLocationButtonEnabled = false;
            _googleMap.UiSettings.MapToolbarEnabled = false;
            _googleMap.UiSettings.CompassEnabled = false;
            _googleMap.UiSettings.ZoomControlsEnabled = false;// this._isZoomGesturesEnabled;

            switch(this._mapType) {
                case MapType.Street: default:
                    _googleMap.MapType = GoogleMap.MapTypeNormal;
                    break;
                case MapType.Satellite:
                    _googleMap.MapType = GoogleMap.MapTypeSatellite;
                    break;
                case MapType.Hybrid:
                    _googleMap.MapType = GoogleMap.MapTypeHybrid;
                    break;
            }

            MoveCamera();

            _googleMap.UiSettings.RotateGesturesEnabled = this._isRotateGesturesEnabled;
            _googleMap.UiSettings.ScrollGesturesEnabled = this._isScrollGesturesEnabled;
            _googleMap.UiSettings.TiltGesturesEnabled = this._isTiltGesturesEnabled;
            _googleMap.UiSettings.ZoomGesturesEnabled = this._isZoomGesturesEnabled;

            //метки
            if (_customPins != null) {
                foreach (var pin in _customPins) {
                    pin.PropertyChanged += CustomPin_PropertyChanged;
                    AddPin(pin);
                }
            }

            //линии
            this._route = DrawRoute();

            _googleMap.MyLocationEnabled = this.FormsMap?.MyLocationEnabled ?? false;
        }

        #endregion

        #region Event handlers

        private void CustomPin_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var pin = (CustomPin)sender;
            var marker = _customPinsBindings[pin];

            UpdatePin(pin, marker, e.PropertyName);
        }

        private void _googleMap_CameraChange(object sender, GoogleMap.CameraChangeEventArgs e) {
            if (this.FormsMap == null) return;

            var newPosition = new Position(e.Position.Target.Latitude, e.Position.Target.Longitude);
            var newZoom = e.Position.Zoom;

            SetFormProperties(() => {
                this.FormsMap.MapCenter = newPosition;
                this.FormsMap.Zoom = newZoom;
            });
            this.MapFunctions?.RaiseCameraPositionChange(newPosition, newZoom);
        }

        private void _googleMap_MarkerClick(object sender, GoogleMap.MarkerClickEventArgs e) {
            if (!this._isShowInfoWindow) return;

            try {
                var binding = _customPinsBindings.First(i => i.Value.Id == e.Marker.Id);

                this._selectedPin = binding.Key;

                SetFormProperties(() => {
                    this.FormsMap.SelectedPin = binding.Key;
                });

                binding.Value.ShowInfoWindow();
            } catch { }
        }

        private void _googleMap_InfoWindowClose(object sender, GoogleMap.InfoWindowCloseEventArgs e) {
            this._selectedPin = null;
            SetFormProperties(() => {
                this.FormsMap.SelectedPin = null;
            });
        }

        private void _googleMap_InfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e) {
            var binding = _customPinsBindings.FirstOrDefault(i => i.Value.Id == e.Marker.Id);

            MapFunctions?.RaiseInfoWindowClick(binding.Key);
        }

        private void _googleMap_MyLocationChange(object sender, GoogleMap.MyLocationChangeEventArgs e) {
            this._userLocation = new LatLng(e.Location.Latitude, e.Location.Longitude);

            if (this.FormsMap == null) return;

            var newPosition = new Position(e.Location.Latitude, e.Location.Longitude);
            this.MapFunctions?.RaiseUserLocationChange(newPosition);
        }

        private void _googleMap_MapClick(object sender, GoogleMap.MapClickEventArgs e) {
            this.MapFunctions?.RaiseMapClick(new Position(e.Point.Latitude, e.Point.Longitude));
        }

        #endregion

        #region InfoWindow render

        private Dictionary<Uri, Android.Graphics.Bitmap> MarkerIconsDownloaded = new Dictionary<Uri, Android.Graphics.Bitmap>();

        public Android.Views.View GetInfoContents(Marker marker) {
            var inflater = Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) as Android.Views.LayoutInflater;
            if (inflater != null) {
                Android.Views.View view;

                var binding = _customPinsBindings.FirstOrDefault(i => i.Value.Id == marker.Id);
                var customPin = binding.Key;

                view = inflater.Inflate(Resource.Layout.MapInfoWindow, null);

                var infoTitle = view.FindViewById<TextView>(Resource.Id.InfoWindowTitle);
                var infoSubtitle = view.FindViewById<TextView>(Resource.Id.InfoWindowSubtitle);
                var infoWindowButton = view.FindViewById<ImageButton>(Resource.Id.InfoWindowButton);

                if (customPin.IconUri != null) {
                    var inCache = MarkerIconsDownloaded.ContainsKey(customPin.IconUri) ? MarkerIconsDownloaded[customPin.IconUri] : null;
                    if (inCache != null) {
                        infoWindowButton.SetMaxHeight(infoWindowButton.Height);
                        infoWindowButton.SetMaxWidth(infoWindowButton.Width);
                        infoWindowButton.SetBackgroundColor(Android.Graphics.Color.Transparent);
                        infoWindowButton.SetImageBitmap(inCache);
                    } else { 
                        Task.Run(async () => {
                            System.IO.Stream download;
                            try {
                                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) })
                                using (var response = await client.GetAsync(customPin.IconUri)) {
                                    download = await response.Content.ReadAsStreamAsync();
                                }
                            } catch {
                                download = null;
                            }

                            if (download != null) {
                                Android.Graphics.Bitmap bitmap;
                                try {
                                    bitmap = await Android.Graphics.BitmapFactory.DecodeStreamAsync(download);
                                } catch {
                                    bitmap = null;
                                }

                                if (bitmap != null) {
                                    MarkerIconsDownloaded.Add(customPin.IconUri, bitmap);

                                    if (this._selectedPin != null && this._selectedPin == customPin) {
                                        Device.BeginInvokeOnMainThread(() => marker.ShowInfoWindow());
                                    }
                                }
                            }

                        });
                    }
                }


                if (infoTitle != null) {
                    infoTitle.Text = marker.Title;
                }
                if (infoSubtitle != null) {
                    infoSubtitle.Text = marker.Snippet;
                }

                return view;
            }
            return null;
        }

        public Android.Views.View GetInfoWindow(Marker marker) {
            return null;
        }

        #endregion

        #region FormMap property changed

        private void FormsMapPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (this._googleMap == null) return;

            if (e.PropertyName == CustomMap.IsCameraAnimatedProperty.PropertyName) { // CameraAnimated
                this._isCameraAnimated = ((CustomMap)sender).IsCameraAnimated;
            } else if (e.PropertyName == CustomMap.MapCenterProperty.PropertyName) { // MapCenter
                this._mapCenter = ((CustomMap)sender).MapCenter;
                ChangeCamera(position: _mapCenter);
            } else if (e.PropertyName == CustomMap.ZoomProperty.PropertyName) { // Zoom
                this._zoom = ((CustomMap)sender).Zoom;
                ChangeCamera(zoom: _zoom);
            } else if (e.PropertyName == CustomMap.IsRotateGesturesEnabledProperty.PropertyName) { // RotateGesture
                this._isRotateGesturesEnabled = ((CustomMap)sender).IsRotateGesturesEnabled;
                this._googleMap.UiSettings.RotateGesturesEnabled = this._isRotateGesturesEnabled;
            } else if (e.PropertyName == CustomMap.IsScrollGesturesEnabledProperty.PropertyName) { // ScrollGesture 
                this._isScrollGesturesEnabled = ((CustomMap)sender).IsScrollGesturesEnabled;
                this._googleMap.UiSettings.ScrollGesturesEnabled = this._isScrollGesturesEnabled;
            } else if (e.PropertyName == CustomMap.IsTiltGesturesEnabledProperty.PropertyName) { // TiltGesture
                this._isTiltGesturesEnabled = ((CustomMap)sender).IsTiltGesturesEnabled;
                this._googleMap.UiSettings.TiltGesturesEnabled = this._isTiltGesturesEnabled;
            } else if (e.PropertyName == CustomMap.IsZoomGesturesEnabledProperty.PropertyName) { // ZoomGesture
                this._isZoomGesturesEnabled = ((CustomMap)sender).IsZoomGesturesEnabled;
                this._googleMap.UiSettings.ZoomGesturesEnabled = this._isZoomGesturesEnabled;
                //this._googleMap.UiSettings.ZoomControlsEnabled = this._isZoomGesturesEnabled;
            } else if (e.PropertyName == CustomMap.SelectedPinProperty.PropertyName) { //SelectedPin
                var pinForm = ((CustomMap)sender).SelectedPin;
                if (pinForm != null) {
                    var marker = _customPinsBindings[pinForm];
                    this._selectedPin = pinForm;
                    marker?.ShowInfoWindow();
                    ((CustomMap)sender).SelectedPin = pinForm;
                } else {
                    if (_selectedPin != null) {
                        var marker = _customPinsBindings[this._selectedPin];
                        marker?.HideInfoWindow();
                        this._selectedPin = null;
                    }
                }
            } else if (e.PropertyName == CustomMap.CustomPinsProperty.PropertyName) { //CustomPins
                //Deleting old collection of pins, unsubscribing from events
                if (this._customPins != null) {
                    this._customPins.CollectionChanged -= CustomPinsCollectionChanged;
                    foreach (var pin in this._customPins) {
                        pin.PropertyChanged -= CustomPin_PropertyChanged;
                        DeletePin(pin);
                    }
                }
                //Addind new collection of pins, subscribing to events
                if (this.FormsMap != null) {
                    this._customPins = this.FormsMap.CustomPins;
                    this._customPinsBindings = new Dictionary<CustomPin, Marker>();
                    if (this._customPins != null) {
                        this._customPins.CollectionChanged += CustomPinsCollectionChanged;
                        foreach (var pin in this._customPins) {
                            pin.PropertyChanged += CustomPin_PropertyChanged;
                            AddPin(pin);
                        }
                    }
                }
            } else if (e.PropertyName == CustomMap.MyLocationEnabledProperty.PropertyName) {
                this._myLocationEnabled = ((CustomMap)sender).MyLocationEnabled;
                _googleMap.MyLocationEnabled = this._myLocationEnabled;
            } else if (e.PropertyName == CustomMap.CustomMapTypeProperty.PropertyName) {
                this._mapType = ((CustomMap)sender).CustomMapType;
                switch (this._mapType) {
                    case MapType.Street:
                    default:
                        _googleMap.MapType = GoogleMap.MapTypeNormal;
                        break;
                    case MapType.Satellite:
                        _googleMap.MapType = GoogleMap.MapTypeSatellite;
                        break;
                    case MapType.Hybrid:
                        _googleMap.MapType = GoogleMap.MapTypeHybrid;
                        break;
                }
            }

        }

        #endregion

    }
}