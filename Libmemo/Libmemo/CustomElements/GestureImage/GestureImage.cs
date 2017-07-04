using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Libmemo {
    public class LongTapImage : Image, LongTapImage.IRenderable {
        public interface IRenderable {
            void LongTapGestureHandler();
            void TapGestureHandler();
        }

        public event EventHandler Tapped;
        public event EventHandler LongTapped;

        void IRenderable.LongTapGestureHandler() => LongTapped?.Invoke(this, new EventArgs());
        void IRenderable.TapGestureHandler() => Tapped?.Invoke(this, new EventArgs());

    }
}
