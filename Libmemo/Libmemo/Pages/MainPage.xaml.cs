﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage {
        public MainPage() {
            InitializeComponent();

            this.menuPage.ListView.ItemSelected += OnMenuItemSelected;
        }

        private void OnMenuItemSelected(object sender, SelectedItemChangedEventArgs e) {
            var item = e.SelectedItem as MenuPageItem;
            if (item != null) {
                Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
                this.menuPage.ListView.SelectedItem = null;
                IsPresented = false;
            }
        }
    }
}
