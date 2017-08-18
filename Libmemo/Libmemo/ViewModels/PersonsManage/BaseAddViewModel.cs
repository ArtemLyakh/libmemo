using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
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
    public abstract class BaseAddViewModel : BaseManagePersonViewModel {

        public BaseAddViewModel() : base() { }

        protected override void Reset() {
            this.LastName = null;
            this.FirstName = null;
            this.SecondName = null;

            this.DateBirth = null;
            this.DateDeath = null;

            this.Text = null;
            this.PhotoSource = null;

            this.Height = null;
            this.Width = null;
            ResetScheme();
        }

        protected override void UserPositionChanged(Position position) {
            base.UserPositionChanged(position);
            this.MapCenter = position;
        }


    }
}
