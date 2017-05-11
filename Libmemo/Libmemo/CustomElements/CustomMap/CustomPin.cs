using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo {
    public class CustomPin : BindableObject {

        #region Visible
        private bool _visible;
        public bool Visible {
            get { return _visible; }
            set {
                if (_visible != value) {
                    _visible = value;
                    this.OnPropertyChanged(nameof(Visible));
                }
            }
        }
        #endregion

        #region Position
        private Position _position;
        public Position Position {
            get { return _position; }
            set {
                if (_position != value) {
                    _position = value;
                    this.OnPropertyChanged(nameof(Position));
                }
            }
        }
        #endregion

        #region Title
        private string _title;
        public string Title {
            get { return _title; }
            set {
                if (_title != value) {
                    _title = value;
                    this.OnPropertyChanged(nameof(Title));
                }
            }
        }
        #endregion

        #region Text
        private string _text;
        public string Text {
            get { return _text; }
            set { _text = value; }
        }
        #endregion

        #region Id
        private string _id;
        public string Id {
            get { return _id; }
            set { _id = value; }
        }
        #endregion

        #region PinImage
        private PinImage _pinImage;
        public PinImage PinImage {
            get { return _pinImage; }
            set {
                if (_pinImage != value) {
                    _pinImage = value;
                    this.OnPropertyChanged(nameof(PinImage));
                }
            }
        }
        #endregion

    }
}
