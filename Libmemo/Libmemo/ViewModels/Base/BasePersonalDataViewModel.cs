using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public abstract class BasePersonalDataViewModel : INotifyPropertyChanged {

        public BasePersonalDataViewModel() { }

        protected abstract PersonData PersonData { get; set; }

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


        public ICommand PickPhotoCommand {
            get => new Command(async () => {
                if (CrossMedia.Current.IsPickPhotoSupported) {
                    MediaFile photo = await CrossMedia.Current.PickPhotoAsync();
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
            get => new Command(Reset);
        }
        protected virtual void Reset() {
            this.FirstName = PersonData?.FirstName;
            this.SecondName = PersonData?.SecondName;
            this.LastName = PersonData?.LastName;
            this.DateBirth = PersonData?.DateBirth;

            this.PhotoSource = PersonData?.PhotoUri == null ? null : new UriImageSource() { CachingEnabled = true, Uri = PersonData.PhotoUri };
        }

        public ICommand SendCommand {
            get => new Command(Send);
        }
        protected abstract void Send();



        protected virtual async Task AddParams(PersonDataLoader uploader) {
            if (!String.IsNullOrWhiteSpace(this.SecondName)) {
                uploader.Params.Add("first_name", this.FirstName.ToString());
            }
            if (!String.IsNullOrWhiteSpace(this.SecondName)) {
                uploader.Params.Add("second_name", this.SecondName.ToString());
            }
            if (!String.IsNullOrWhiteSpace(this.LastName)) {
                uploader.Params.Add("last_name", this.LastName.ToString());
            }
            if (this.DateBirth.HasValue) {
                uploader.Params.Add("date_birth", this.DateBirth.Value.ToString("yyyy-MM-dd"));
            }
            if (this.PhotoSource != null && this.PhotoSource is FileImageSource) {
                await uploader.SetFile(this.PhotoSource);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
