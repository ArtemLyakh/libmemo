using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage {

        public ListView ListView { get { return this.listView; } }


        public ObservableCollection<MenuPageItem> MenuList { get; set; } = new ObservableCollection<MenuPageItem>();
        private string _userEmail;
        public string UserEmail {
            get { return _userEmail; }
            set {
                if (_userEmail != value) {
                    _userEmail = value;
                    OnPropertyChanged(nameof(UserEmail));
                }
            }
        }

        private bool _isUserEmailVisible;
        public bool IsUserEmailVisible {
            get { return _isUserEmailVisible; }
            set {
                if (_isUserEmailVisible != value) {
                    _isUserEmailVisible = value;
                    OnPropertyChanged(nameof(IsUserEmailVisible));
                }
            }
        }


        public MenuPage() {
            InitializeComponent();

            SetMenuPage();

            this.BindingContext = this;
        }


        public void ExecuteMenuItem(string title) {
            var item = MenuList.FirstOrDefault(i => i.Title.Equals(title));
            if (item == null) throw new ArgumentException($"Не найден пункт \"{title}\"");
            App.GlobalPage.ExecuteMenuItem(item);
        }


        public void SetMenuPage() {

            UserEmail = Settings.Email;
            IsUserEmailVisible = AuthHelper.IsLogged();

            MenuList.Clear();
            foreach (var item in GetMenuList()) {
                MenuList.Add(item);
            }
        }



        private static IEnumerable<MenuPageItem> GetMenuList() {
            bool isLogged = AuthHelper.IsLogged();

            yield return new MenuPageItem {
                Title = "Карта",
                Text = "карта",
                Page = typeof(MapPage)
            };
            yield return new MenuPageItem {
                Title = "Сбросить базу данных",
                Text = "Полное обновление базы данных",
                Action = () => {
                    App.ToastNotificator.Show("Скачивание данных");
                    App.Database.Load(true);
                }
            };

            if (isLogged) {
                yield return new MenuPageItem {
                    Title = "Добавить",
                    Text = "добавить",
                    Page = typeof(AddPage)
                };
                yield return new MenuPageItem {
                    Title = "Редактировать данные",
                    Text = "Редактирование персональных данных",
                    Page = typeof(PersonalDataPage)
                };
                yield return new MenuPageItem {
                    Title = "Выйти",
                    Action = () => {
                        AuthHelper.Logout();
                    }
                };
            } else {
                yield return new MenuPageItem {
                    Title = "Авторизация",
                    Page = typeof(LoginPage)
                };
                yield return new MenuPageItem {
                    Title = "Регистрация",
                    Page = typeof(RegisterPage)
                };
            }

        }
    }

    public class MenuPageItem {
        public string Title { get; set; }
        public string Text { get; set; }
        public Type Page { get; set; }
        public Action Action { get; set; }
    }
}
