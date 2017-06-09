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

        public PersonalDataPageViewModel() : base() {
            LoadData();
        }

        protected override PersonData PersonData { get; set; } = null;

        protected async void LoadData() {
            var loader = new PersonDataLoader(Settings.PersonalDataUrl);

            try {
                PersonData = await loader.GetPersonData();
            } catch (UnauthorizedAccessException) {
                await AuthHelper.ReloginAsync();
                return;
            }

            if (PersonData == null) {
                App.ToastNotificator.Show("Ошибка загрузки текущих данных");
            } else {
                App.ToastNotificator.Show("Данные с сервера получены");
                this.ResetCommand.Execute(null);
            }

        }

        protected override async void Send() {
            PersonDataLoader uploader = new PersonDataLoader(Settings.PersonalDataUrl);
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
                await AuthHelper.ReloginAsync();
            }

        }



    }
}
