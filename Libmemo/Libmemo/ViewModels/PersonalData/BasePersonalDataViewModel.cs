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
    public abstract class BasePersonalDataViewModel : BaseViewModel {

        public BasePersonalDataViewModel() { }

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


        public ICommand PickPhotoCommand => new Command(async () => {
            if (CrossMedia.Current.IsPickPhotoSupported) {
                MediaFile photo = await CrossMedia.Current.PickPhotoAsync();
                if (photo == null) return;
                this.PhotoSource = ImageSource.FromFile(photo.Path);
            }
        });


        public ICommand MakePhotoCommand => new Command(async () => {
            if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported) {
                var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { SaveToAlbum = false });
                if (file == null) return;
                PhotoSource = ImageSource.FromFile(file.Path);
            }
        });

        protected abstract void Reset();
        public ICommand ResetCommand => new Command(Reset);

        protected abstract void Send();
        public ICommand SendCommand => new Command(Send);

    }
}
