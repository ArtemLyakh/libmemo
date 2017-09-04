using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo.Pages
{
    public partial class TreeList : ContentPage
    {
		private ViewModel Model
		{
			get => (ViewModel)BindingContext;
			set => BindingContext = value;
		}

        public TreeList(List<Json.PersonDetail.Tree> list)
		{
			InitializeComponent();
			BindingContext = new ViewModel(list);
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

            public ViewModel(List<Json.PersonDetail.Tree> list) : base()
			{
                Data = list.Select(i => new Entry(i.id, i.fio)).ToList();
			}

			public class Entry
			{
				public int Id { get; set; }
				public string Fio { get; set; }

				public Entry(int id, string fio)
				{
					Id = id;
                    Fio = fio;
				}
			}

			private List<Entry> _data;
			public List<Entry> Data
			{
				get => _data;
				set
				{
					_data = value;
					OnPropertyChanged(nameof(Data));
				}
			}

			public ICommand ItemSelectedCommand => new Command<object>(selected => {
				var entry = (Entry)selected;
                App.ToastNotificator.Show($"id: {entry.Id}");
			});
		}
    }
}
