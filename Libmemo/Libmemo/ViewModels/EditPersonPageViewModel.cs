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
    public class EditPersonPageViewModel : INotifyPropertyChanged {

        private int id;
        private bool isChanged = false;
        public EditPersonPageViewModel(int id) {
            this.id = id;
        }

        public async void Init() {
            var person = await App.Database.GetById<Person>(id);

            FirstName = person.FirstName;
            SecondName = person.SecondName;
            SecondName = person.LastName;
            DateBirth = person.DateBirth;
            DateDeath = person.DateDeath;
            Text = person.Text;
            if (Uri.TryCreate(person.ImageUrl, UriKind.Absolute, out Uri imageUri))
                PhotoSource = new UriImageSource() { CachingEnabled = true, Uri = imageUri };

            isChanged = false;
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

        private DateTime? _dateDeath = null;
        public DateTime? DateDeath {
            get { return _dateDeath; }
            set {
                if (_dateDeath != value) {
                    _dateDeath = value;
                    this.OnPropertyChanged(nameof(DateDeath));
                }
            }
        }

        private string _text;
        public string Text {
            get { return _text; }
            set {
                if (_text != value) {
                    _text = value;
                    this.OnPropertyChanged(nameof(Text));
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
            get => new Command(() => Init());
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = "") {
            isChanged = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
