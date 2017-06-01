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
    class PersonalDataPageViewModel : INotifyPropertyChanged {

        private PersonData _personData = null;
        public PersonalDataPageViewModel() {
            LoadData();
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
            get {
                return new Command(async () => {
                    if (CrossMedia.Current.IsPickPhotoSupported) {
                        MediaFile photo = await CrossMedia.Current.PickPhotoAsync();
                        if (photo == null) return;
                        this.PhotoSource = ImageSource.FromFile(photo.Path);
                    }
                });
            }
        }

        public ICommand MakePhotoCommand {
            get {
                return new Command(async () => {
                    if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported) {
                        string fio = String.Join("_", new string[] { this.LastName, this.FirstName, this.SecondName }).Trim();
                        fio = String.IsNullOrWhiteSpace(fio)
                            ? $"{DateTime.Now.ToString("dd.MM.yyyy_hh.mm.ss")}.jpg"
                            : fio + ".jpg";

                        MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions {
                            SaveToAlbum = true,
                            Directory = "RitualUg",
                            Name = fio
                        });

                        if (file == null)
                            return;

                        PhotoSource = ImageSource.FromFile(file.Path);
                    }
                });
            }
        }

        public ICommand ResetCommand {
            get {
                return new Command(() => {

                    this.FirstName = _personData?.FirstName;
                    this.SecondName = _personData?.SecondName;
                    this.LastName = _personData?.LastName;
                    this.DateBirth = _personData?.DateBirth;

                    this.PhotoSource = null;
                });
            }
        }

        public ICommand SendCommand {
            get {
                return new Command(async () => {
                    PersonDataLoader uploader = new PersonDataLoader(Settings.PersonalDataSend);

                    if (_personData?.FirstName == this.FirstName
                        && _personData?.SecondName == this.SecondName
                        && _personData?.LastName == this.LastName
                        && _personData?.DateBirth == this.DateBirth
                        && this.PhotoSource == null
                    ) {
                        App.ToastNotificator.Show("Отсутствуют изменения");
                        return;
                    } else {
                        App.ToastNotificator.Show("Отправка на сервер");
                    }

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

                    try {
                        var success = await uploader.Upload();

                        if (success) {
                            App.ToastNotificator.Show("Данные успешно отправлены");
                            LoadData();
                        } else {
                            Device.BeginInvokeOnMainThread(async () => {
                                await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка отправки данных", "ОК");
                            });
                        }
                    } catch (UnauthorizedAccessException) {
                        AuthHelper.Relogin();
                    }

                });
            }
        }


        private async void LoadData() {
            var loader = new PersonDataLoader(Settings.PersonalDataGet);

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



        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
