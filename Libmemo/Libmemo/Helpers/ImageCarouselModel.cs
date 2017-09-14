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

namespace Libmemo.Helpers
{
    class ImageCarouselModel
    {
        public class Image : INotifyPropertyChanged
        {
            public Image() { }

            public event EventHandler PhotoSelected;
            public event EventHandler PhotoFirstTimeSelected;

            private ImageSource _photoSource;
            public ImageSource PhotoSource {
                get => _photoSource;
                set {
                    if (_photoSource != value) {
                        _photoSource = value;
                        this.OnPropertyChanged(nameof(PhotoSource));
                    }
                }
            }

            public ICommand PickPhotoCommand => new Command(async () => {
                if (CrossMedia.Current.IsPickPhotoSupported) {
                    var photo = await CrossMedia.Current.PickPhotoAsync();
                    if (photo == null) return;

                    var first = false;
                    if (PhotoSource == null) first = true;

                    this.PhotoSource = ImageSource.FromFile(photo.Path);

                    PhotoSelected?.Invoke(this, null);
                    if (first) PhotoFirstTimeSelected?.Invoke(this, null);
                } else {
                    App.ToastNotificator.Show("Выбор фото невозможен");
                }
            });
            public ICommand MakePhotoCommand => new Command(async () => {
                if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported) {
                    var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { SaveToAlbum = false });
                    if (file == null) return;

                    var first = false;
                    if (PhotoSource == null) first = true;

                    PhotoSource = ImageSource.FromFile(file.Path);

                    PhotoSelected?.Invoke(this, null);
                    if (first) PhotoFirstTimeSelected?.Invoke(this, null);
                } else {
                    App.ToastNotificator.Show("Сделать фото невозможно");
                }
            });


            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName = "") =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private List<ImageSource> photoList = new List<ImageSource>();

        public List<ImageSource> PhotoSize => photoList;
        //public Obse







    }
}
