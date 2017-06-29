using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public abstract class BaseEditPersonViewModel : BaseManagePersonViewModel {

        protected int Id { get; set; }
        public BaseEditPersonViewModel(int id) : base() {
            this.Id = id;

            CustomPins.Add(new CustomPin() {
                PinImage = PinImage.Default,
                Id = id.ToString(),
                Position = new Position(),
                Visible = true
            });

            this.ResetCommand.Execute(null);     
        }


        public INativeMapFunction MapFunctions { get; set; }

        private ObservableCollection<CustomPin> _customPins = new ObservableCollection<CustomPin>();
        public ObservableCollection<CustomPin> CustomPins {
            get { return _customPins; }
            set {
                if (_customPins != value) {
                    _customPins = value;
                    this.OnPropertyChanged(nameof(CustomPins));
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
                    _personPosition = value;

                    var pin = CustomPins.FirstOrDefault();
                    if (pin != null) pin.Position = value;

                    OnPropertyChanged(nameof(PersonLatLon));
                }
            }
        }

        public ICommand MapClickCommand {
            get => new Command<Position>((Position position) => PersonPosition = position);
        }

        public ICommand CurrentGeoCommand {
            get => new Command(() => {
                if (UserPosition.HasValue) {
                    PersonPosition = UserPosition.Value;
                }
            });
        }

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
            get => new Command(() => {
                IsLatLonShow = !IsLatLonShow;
                if (IsLatLonShow) {
                    if (this.PersonPosition != default(Position))
                        this.MapCenter = this.PersonPosition;
                    else if (this.UserPosition.HasValue)
                        this.MapCenter = this.UserPosition.Value;
                    else
                        this.UserPositionChangedFirstTime += (object sender, Position position) => this.MapCenter = position;
                }
            });
        }


        private Uri _schemeUrl = null;
        protected Uri SchemeUrl {
            get => _schemeUrl;
            set {
                if (this._schemeUrl != value) {
                    this._schemeUrl = value;
                    this.OnPropertyChanged(nameof(IsSchemeCanDownload));
                }
            }
        }
        public bool IsSchemeCanDownload => SchemeUrl != null;

        public ICommand SchemeDownloadCommand {
            get => new Command(() =>
                Device.OpenUri(SchemeUrl)
            );
        }

        


        public ICommand GoToGeoCommand => new Command(() => {
            if (this.UserPosition.HasValue)
                this.MapCenter = this.UserPosition.Value;
        });








        protected override async void Reset() {
            var person = await App.Database.GetById(Id);

            this.Type = person.PersonType;

            this.FirstName = person.FirstName;
            this.SecondName = person.SecondName;
            this.LastName = person.LastName;
            this.DateBirth = person.DateBirth;
            this.PhotoSource = person.ImageUrl != null ? new UriImageSource { CachingEnabled = true, Uri = person.ImageUrl } : null;
            
            if (person.PersonType == PersonType.Dead) {
                this.PersonPosition = new Position(person.Latitude, person.Longitude);
                this.MapCenter = this.PersonPosition;

                this.DateDeath = person.DateDeath;
                this.Text = person.Text;
                this.Height = person.Height;
                this.Width = person.Width;
                this.SchemeUrl = person.SchemeUrl;
            }

        }





        //protected virtual IEnumerable<string> Validate() {
        //    if (String.IsNullOrWhiteSpace(this.FirstName)) yield return "Поле \"Имя\" не заполнено";
        //}

        //protected virtual async Task AddParams(PersonDataLoader uploader) {
        //    uploader.Params.Add("id", this.Id.ToString());

        //    uploader.Params.Add("latitude", this.PersonPosition.Latitude.ToString(CultureInfo.InvariantCulture));
        //    uploader.Params.Add("longitude", this.PersonPosition.Longitude.ToString(CultureInfo.InvariantCulture));

        //    if (!string.IsNullOrWhiteSpace(this.FirstName)) {
        //        uploader.Params.Add("first_name", this.FirstName.ToString());
        //    }
        //    if (!string.IsNullOrWhiteSpace(this.SecondName)) {
        //        uploader.Params.Add("second_name", this.SecondName.ToString());
        //    }
        //    if (!string.IsNullOrWhiteSpace(this.LastName)) {
        //        uploader.Params.Add("last_name", this.LastName.ToString());
        //    }

        //    if (this.DateBirth.HasValue) {
        //        uploader.Params.Add("date_birth", this.DateBirth.Value.ToString("yyyy-MM-dd"));
        //    }
        //    if (this.DateDeath.HasValue) {
        //        uploader.Params.Add("date_death", this.DateDeath.Value.ToString("yyyy-MM-dd"));
        //    }
        //    if (!string.IsNullOrWhiteSpace(this.Text)) {
        //        uploader.Params.Add("text", this.Text.ToString());
        //    }
        //    if (this.PhotoSource != null && this.PhotoSource is FileImageSource) {
        //        await uploader.SetFile(this.PhotoSource);
        //    }

        //    if (this.Height.HasValue) {
        //        uploader.Params.Add("height", this.Height.Value.ToString(CultureInfo.InvariantCulture));
        //    }
        //    if (this.Width.HasValue) {
        //        uploader.Params.Add("width", this.Width.Value.ToString(CultureInfo.InvariantCulture));
        //    }

        //    if (this.SchemeStream != null) {
        //        uploader.Files.Add("scheme", Tuple.Create(this.SchemeName, this.SchemeStream));
        //    }
        //}

    }
}
