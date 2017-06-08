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
    
        public BasePersonalDataViewModel() {
            LoadData();
        }
        public BasePersonalDataViewModel(int id) {
            LoadData(id);
        }


        protected PersonData _personData = null;
        protected async void LoadData(int? id = null) {
            var loader = new PersonDataLoader(id.HasValue ? id.Value.ToString() : Settings.PersonalDataGet);

            try {
                _personData = await loader.GetPersonData();
            } catch (UnauthorizedAccessException) {
                AuthHelper.Relogin();
                return;
            }

            if (_personData == null) {
                App.ToastNotificator.Show("Ошибка загрузки текущих данных");
            } else {
                App.ToastNotificator.Show("Данные с сервера получены");
                this.ResetCommand.Execute(null);
            }

        }


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
            this.FirstName = _personData?.FirstName;
            this.SecondName = _personData?.SecondName;
            this.LastName = _personData?.LastName;
            this.DateBirth = _personData?.DateBirth;

            this.PhotoSource = null;
        }

        public ICommand SendCommand {
            get => new Command(Send);
        }
        protected abstract void Send();



        protected virtual bool IsSomethingChanged() {
            return _personData?.FirstName != this.FirstName
                || _personData?.SecondName != this.SecondName
                || _personData?.LastName != this.LastName
                || _personData?.DateBirth != this.DateBirth
                || this.PhotoSource != null;
        }

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
            if (this.PhotoSource != null) {
                await uploader.SetFile(this.PhotoSource);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
