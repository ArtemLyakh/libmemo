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


        public ObservableCollection<MenuItem> MenuList { get; set; } = new ObservableCollection<MenuItem>();
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


        public void ExecuteMenuItem(MenuItemId id) {
            var item = MenuList.FirstOrDefault(i => i.Id.Equals(id))
                ?? throw new ArgumentException($"Не найден пункт \"{id}\"");
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



        private static IEnumerable<MenuItem> GetMenuList() {
            yield return new MenuItem {
                Id = MenuItemId.Map,
                Title = "Карта",
                Text = "карта",
                Action = async () => { await App.GlobalPage.PopToRootPage(); }
            };
            yield return new MenuItem {
                Id = MenuItemId.ReloadDatabase,
                Title = "Сбросить базу данных",
                Text = "Полное обновление базы данных",
                Action = () => {
                    App.ToastNotificator.Show("Скачивание данных");
                    App.Database.Load(true);
                }
            };

            if (AuthHelper.IsLogged) {
                if (AuthHelper.IsAdmin) {
                    yield return new MenuItem {
                        Id = MenuItemId.AddAdmin,
                        Title = "Добавить/админ",
                        Text = "админ",
                        Action = async () => { await App.GlobalPage.PushRoot(new AddPageAdmin()); }
                    };
                    yield return new MenuItem {
                        Id = MenuItemId.RegisterAdmin,
                        Title = "Зарегистрировать пользователя",
                        Text = "Админ",
                        Action = async () => { await App.GlobalPage.PushRoot(new RegisterAdminPage()); }
                    };
                    yield return new MenuItem {
                        Id = MenuItemId.UserDataAdmin,
                        Title = "Редактировать данные",
                        Text = "Редактировать данные пользователей",
                        Action = async () => { await App.GlobalPage.PushRoot(new PersonalDataPageAdmin()); }
                    };
                } else {
                    yield return new MenuItem {
                        Id = MenuItemId.Add,
                        Title = "Добавить",
                        Text = "добавить",
                        Action = async () => { await App.GlobalPage.PushRoot(new AddPage()); }
                    };
                    yield return new MenuItem {
                        Id = MenuItemId.UserData,
                        Title = "Редактировать данные",
                        Text = "Редактирование персональных данных",
                        Action = async () => { await App.GlobalPage.PushRoot(new PersonalDataPage()); }
                    };
                }


                yield return new MenuItem {
                    Id = MenuItemId.Exit,
                    Title = "Выйти",
                    Action = () => AuthHelper.Logout()
                };
            } else {
                yield return new MenuItem {
                    Id = MenuItemId.Login,
                    Title = "Авторизация",
                    Action = async () => { await App.GlobalPage.PushRoot(new LoginPage()); }
                };
                yield return new MenuItem {
                    Id = MenuItemId.Register,
                    Title = "Регистрация",
                    Action = async () => { await App.GlobalPage.PushRoot(new RegisterPage()); }
                };
            }

        }
    }

    public class MenuItem {
        public MenuItemId Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public Action Action { get; set; }
    }

    public enum MenuItemId {
        Map, ReloadDatabase, Add, AddAdmin, UserData, UserDataAdmin, Exit, Login, Register, RegisterAdmin
    }
}
