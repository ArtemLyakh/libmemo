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
    public class PersonalDataPageViewModel : BasePersonalDataViewModel {

        public PersonalDataPageViewModel() : base() { }



        protected override async void Send() {
            if (!IsSomethingChanged()) {
                App.ToastNotificator.Show("Отсутствуют изменения");
                return;
            } else {
                App.ToastNotificator.Show("Отправка на сервер");
            }

            PersonDataLoader uploader = new PersonDataLoader(Settings.PersonalDataSend);
            await AddParams(uploader);

            try {
                var success = await uploader.Upload();

                if (success) {
                    App.ToastNotificator.Show("Данные успешно отправлены");
                    LoadData();
                } else {
                    Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка отправки данных", "ОК"));
                }
            } catch (UnauthorizedAccessException) {
                AuthHelper.Relogin();
            }

        }



    }
}
