﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage {

        public ListView ListView { get { return this.listView; } }

        public class MenuItem {
            public ImageSource Image { get; set; }
            public string Text { get; set; }
            public Func<Task> Action { get; set; }
        }

        public ObservableCollection<MenuItem> MenuList { get; set; } = new ObservableCollection<MenuItem>();


        public MenuPage() {
            InitializeComponent();
            SetMenuPage();
            this.BindingContext = this;
        }

        public void SetMenuPage() {
            MenuList.Clear();
            foreach (var item in GetMenuList()) {
                MenuList.Add(item);
            }
            OnPropertyChanged(nameof(IsLogged));
            OnPropertyChanged(nameof(IsAdmin));
        }



        private static IEnumerable<MenuItem> GetMenuList() {
            yield return new MenuItem {
                Text = "Карта",
                Image = ImageSource.FromFile("menu_map"),
                Action = () => App.GlobalPage.PopToRootPage()
            };
            yield return new MenuItem {
                Text = "Синхронизация",
                Image = ImageSource.FromFile("menu_sync"),
                Action = () => Task.Run(() => Device.BeginInvokeOnMainThread(() => {
                    App.ToastNotificator.Show("Скачивание данных");
                    App.Database.Load(true);
                }))
            };

            if (!AuthHelper.IsLogged) {
                yield return new MenuItem {
                    Text = "Авторизация",
                    Image = ImageSource.FromFile("menu_login"),
                    Action = () => App.GlobalPage.PushRoot(new LoginPage())
                };
                yield return new MenuItem {
                    Text = "Регистрация",
                    Image = ImageSource.FromFile("menu_reg"),
                    Action = () => App.GlobalPage.PushRoot(new RegisterPage())
                };
            } else {
                if (AuthHelper.IsAdmin) {
                    yield return new MenuItem {
                        Text = "Родственники пользователей",
                        Image = ImageSource.FromFile("menu_rel"),
                        Action = () => App.GlobalPage.PushRoot(new PersonCollectionAdminPage())
                    };
                    yield return new MenuItem {
                        Text = "Список пользователей",
                        Image = ImageSource.FromFile("menu_login"),
                        Action = () => {
                            var page = new UserListPage();
                            page.ItemSelected += async (object sender, UserListPageViewModel.User user) =>
                                await App.GlobalPage.PushRoot(new PersonalDataPageAdmin(user.Id));
                            return App.GlobalPage.PushRoot(page);
                        }
                    };
                    yield return new MenuItem {
                        Text = "Деревья пользователей",
                        Image = ImageSource.FromFile("menu_tree"),
                        Action = () => {
                            var page = new UserListPage();
                            page.ItemSelected += async (object sender, UserListPageViewModel.User user) =>
                                await App.GlobalPage.PushRoot(new TreePageAdmin(user.Id));
                            return App.GlobalPage.PushRoot(page);
                        }
                    };
                } else {
                    yield return new MenuItem {
                        Text = "Родственники",
                        Image = ImageSource.FromFile("menu_rel"),
                        Action = () => App.GlobalPage.PushRoot(new PersonCollectionPage())
                    };
                    yield return new MenuItem {
                        Text = "Древо",
                        Image = ImageSource.FromFile("menu_tree"),
                        Action = () => App.GlobalPage.PushRoot(new TreePage())
                    };
                }

                yield return new MenuItem {
                    Text = "Выйти",
                    Image = ImageSource.FromFile("menu_exit"),
                    Action = () => AuthHelper.Logout()
                };
            }
        }

        public ICommand BackCommand => new Command(() => App.SetShowMenu(false));

        public ICommand LKCommand => new Command(async () => {
            if (!AuthHelper.IsLogged || AuthHelper.IsAdmin) return;
            await App.GlobalPage.PushRoot(new PersonalDataPage());
            App.SetShowMenu(false);
        });

        public bool IsLogged => AuthHelper.IsLogged;
        public bool IsAdmin => AuthHelper.IsAdmin;
    }
}
