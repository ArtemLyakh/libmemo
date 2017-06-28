using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo {
    class AddPageViewModel : BaseAddViewModel {

        public AddPageViewModel() : base() { }

        protected override async void Send() {
            var errors = Validate();
            if (errors.Count() > 0) {
                Device.BeginInvokeOnMainThread(() => App.Current.MainPage.DisplayAlert("Ошибка", string.Join("\n", errors), "ОК"));
                return;
            }

            App.ToastNotificator.Show("Отправка на сервер");

            var uploader = new PersonDataLoader(Settings.ADD_PERSON_URL);
            await AddParams(uploader);

            try {
                var success = await uploader.Upload();

                if (success) {
                    App.ToastNotificator.Show("Данные успешно отправлены");
                    ResetCommand.Execute(null);
                    App.Database.Load();
                } else {
                    Device.BeginInvokeOnMainThread(() => App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка отправки данных", "ОК"));
                }
            } catch (UnauthorizedAccessException) {
                await AuthHelper.ReloginAsync();
            }
        }

    }

}