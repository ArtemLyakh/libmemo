using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Libmemo {
    public static class UtilsFunctions {
        public static async Task GetGPSPermissionOrExit() {
            var location = await Plugin.Permissions.CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Location);

            if (location != Plugin.Permissions.Abstractions.PermissionStatus.Granted) {
                var results = await Plugin.Permissions.CrossPermissions.Current.RequestPermissionsAsync(new[] { Plugin.Permissions.Abstractions.Permission.Location });
                var status = results[Plugin.Permissions.Abstractions.Permission.Location];
                if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted) {
                    Device.BeginInvokeOnMainThread(async () => {
                        await App.Current.MainPage.DisplayAlert("Ошибка", "Приложению требуется доступ к геолокации", "Завершить работу");
                        DependencyService.Get<ICloseApplication>().CloseApplication();
                    });
                }
            }
        }
    }
}
