﻿using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {

    public class CustomListView : Xamarin.Forms.ListView {

        public static readonly BindableProperty ItemClickCommandProperty = BindableProperty.Create(
            nameof(ItemClickCommand),
            typeof(Command<Tuple<int, string>>),
            typeof(CustomListView));

        public ICommand ItemClickCommand {
            get { return (ICommand)this.GetValue(ItemClickCommandProperty); }
            set { this.SetValue(ItemClickCommandProperty, value); }
        }



        public CustomListView() {
            this.ItemTapped += this.OnItemTapped;
        }




        private object prevTapped = null;
        private void OnItemTapped(object sender, ItemTappedEventArgs e) {

            if (this.prevTapped != e.Item) {
                this.prevTapped = e.Item;
                return;
            } else {
                if (e.Item != null && this.ItemClickCommand != null && this.ItemClickCommand.CanExecute(e.Item)) {
                    this.ItemClickCommand.Execute(e.Item);
                    this.SelectedItem = null;
                    this.prevTapped = null;

                }
            }

        }
    }
}
