﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetailPage : ContentPage {

        public DetailPage(int id) {
            InitializeComponent();
            this.BindingContext = new DetailPageViewModel(id);
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            (BindingContext as DetailPageViewModel)?.Init();
        }

    }
}
