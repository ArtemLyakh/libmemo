using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Test : ContentPage
    {
		private ViewModel Model
		{
			get => (ViewModel)BindingContext;
			set => BindingContext = value;
		}

		public Test()
		{
            InitializeComponent();
			BindingContext = new ViewModel();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Model.OnAppearing();
		}
		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			Model.OnDisappearing();
		}

        public class ViewModel : BaseViewModel
        {
            public ViewModel() :base ()
            {
                Data = new ObservableCollection<Entry>()
                {
                    new Entry() {
                        Text = "qqq",
                        Command1 = new Command(() => App.ToastNotificator.Show("1")),
                        Command2 = new Command(() => App.ToastNotificator.Show("2"))
                    },
					new Entry() {
						Text = "www",
						Command1 = new Command(() => App.ToastNotificator.Show("3")),
						Command2 = new Command(() => App.ToastNotificator.Show("4"))
					}
                };
            }

            public class Entry
            {
                public string Text { get; set; }
                public ICommand Command1 { get; set; }
                public ICommand Command2 { get; set; }
            }

            public ObservableCollection<Entry> Data { get; set; }
        }
    }
}
