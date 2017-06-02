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

        private bool _isUserAdminVisible;
        public bool IsUserAdminVisible {
            get { return _isUserAdminVisible; }
            set {
                if (_isUserAdminVisible != value) {
                    _isUserAdminVisible = value;
                    OnPropertyChanged(nameof(IsUserAdminVisible));
                }
            }
        }

        public MenuPage() {
            InitializeComponent();

            SetMenuPage();

            this.BindingContext = this;
        }


        public void ExecuteMenuItem(string id) {
            var item = MenuList.FirstOrDefault(i => i.Id.Equals(id));
            if (item == null) throw new ArgumentException($"Не найден пункт \"{id}\"");
            App.GlobalPage.ExecuteMenuItem(item);
        }


        public void SetMenuPage() {

            UserEmail = Settings.Email;
            IsUserEmailVisible = AuthHelper.IsLogged;
            IsUserAdminVisible = AuthHelper.IsAdmin;

            MenuList.Clear();
            foreach (var item in GetMenuList()) {
                MenuList.Add(item);
            }
        }



        private static IEnumerable<MenuPageItem> GetMenuList() {
            yield return new MenuPageItem {
                Id = "map",
                Title = "Карта",
                Text = "карта",
                Page = typeof(MapPage)
            };
            yield return new MenuPageItem {
                Id = "reload",
                Title = "Сбросить базу данных",
                Text = "Полное обновление базы данных",
                Action = () => {
                    App.ToastNotificator.Show("Скачивание данных");
                    App.Database.Load(true);
                }
            };

            if (AuthHelper.IsLogged) {
                if (AuthHelper.IsAdmin) {
                    yield return new MenuPageItem {
                        Id = "add_admin",
                        Title = "Добавить/админ",
                        Text = "админ",
                        Page = typeof(AddPageAdmin)
                    };
                } else {
                    yield return new MenuPageItem {
                        Id = "add",
                        Title = "Добавить",
                        Text = "добавить",
                        Page = typeof(AddPage)
                    };
                }

                yield return new MenuPageItem {
                    Id = "edit",
                    Title = "Редактировать данные",
                    Text = "Редактирование персональных данных",
                    Page = typeof(PersonalDataPage)
                };
                yield return new MenuPageItem {
                    Id = "exit",
                    Title = "Выйти",
                    Action = () => {
                        AuthHelper.Logout();
                    }
                };
            } else {
                yield return new MenuPageItem {
                    Id = "login",
                    Title = "Авторизация",
                    Page = typeof(LoginPage)
                };
                yield return new MenuPageItem {
                    Id = "register",
                    Title = "Регистрация",
                    Page = typeof(RegisterPage)
                };
            }

        }
    }

    public class MenuPageItem {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public Type Page { get; set; }
        public Action Action { get; set; }
    }
}
