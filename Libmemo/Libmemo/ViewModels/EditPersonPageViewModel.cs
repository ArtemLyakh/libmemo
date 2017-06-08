using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo {
    public class EditPersonPageViewModel : BaseEditPersonViewModel {

        public EditPersonPageViewModel(int id) : base(id) { }

        protected override async void Send() {
            var errors = Validate();
            if (errors.Count() > 0) {
                Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", string.Join("\n", errors), "ОК"));
                return;
            }

            App.ToastNotificator.Show("Отправка на сервер");

            PersonDataLoader uploader = new PersonDataLoader(Settings.EditPersonUrl);

            await AddParams(uploader);

            try {
                var success = await uploader.Upload();

                if (success) {
                    App.ToastNotificator.Show("Данные успешно отправлены");
                    App.Database.Load();
                    App.MenuPage.ExecuteMenuItem(MenuItemId.Map);
                } else {
                    Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка отправки данных", "ОК"));
                }
            } catch (UnauthorizedAccessException) {
                AuthHelper.Relogin();
            }
        }  

    }
}
