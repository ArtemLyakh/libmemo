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
                Title = "Карта",
                Text = "карта",
                Action = () => App.GlobalPage.PopToRootPage()
            };
            yield return new MenuItem {
                Title = "Сбросить базу данных",
                Text = "Полное обновление базы данных",
                Action = () => Task.Run(() => {
                    App.ToastNotificator.Show("Скачивание данных");
                    App.Database.Load(true);
                })
            };

            if (AuthHelper.IsLogged) {
                if (AuthHelper.IsAdmin) {
                    yield return new MenuItem {
                        Title = "Добавить/админ",
                        Text = "админ",
                        Action = () => App.GlobalPage.PushRoot(new AddPageAdmin())
                    };
                    yield return new MenuItem {
                        Title = "Зарегистрировать пользователя",
                        Text = "Админ",
                        Action = () => App.GlobalPage.PushRoot(new RegisterAdminPage())
                    };
                    yield return new MenuItem {
                        Title = "Редактировать данные",
                        Text = "Редактировать данные пользователей",
                        Action = () => App.GlobalPage.PushRoot(new PersonalDataPageAdmin())
                    };
                } else {
                    yield return new MenuItem {
                        Title = "Добавить",
                        Text = "добавить",
                        Action = () => App.GlobalPage.PushRoot(new AddPage())
                    };
                    yield return new MenuItem {
                        Title = "Редактировать данные",
                        Text = "Редактирование персональных данных",
                        Action = () => App.GlobalPage.PushRoot(new PersonalDataPage())
                    };
                    yield return new MenuItem {
                        Title = "Древо",
                        Text = "Редактирование генеологического древа",
                        Action = () => App.GlobalPage.PushRoot(new TreePage())
                    };
                }


                yield return new MenuItem {
                    Title = "Выйти",
                    Action = () => AuthHelper.Logout()
                };
            } else {
                yield return new MenuItem {
                    Title = "Авторизация",
                    Action = () => App.GlobalPage.PushRoot(new LoginPage())
                };
                yield return new MenuItem {
                    Title = "Регистрация",
                    Action = () => App.GlobalPage.PushRoot(new RegisterPage())
                };
            }

        }
    }

    public class MenuItem {
        public string Title { get; set; }
        public string Text { get; set; }
        public Func<Task> Action { get; set; }
    }

}
