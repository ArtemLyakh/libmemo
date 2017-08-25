using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RelativesPage : ContentPage {
		private ViewModel Model
		{
			get => (ViewModel)BindingContext;
			set => BindingContext = value;
		}

		public RelativesPage()
		{
			InitializeComponent();
			BindingContext = new PersonCollectionPageViewModel();
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

		public class ViewModel : BaseListViewModel<ListElement.ImageElement>
		{

			public ViewModel() : base()
			{
				this.ItemSelected += async (sender, e) => await App.GlobalPage.Push(new EditPersonPage(e.Person.Id));
			}

			public ICommand BackCommand => new Command(async () => await App.GlobalPage.Pop());

			public ICommand AddCommand => new Command(async () => await App.GlobalPage.Push(new AddPage()));

			public ICommand LoadCommand => new Command(async () => await Load());

			private async Task Load()
			{
				if (!AuthHelper.IsLogged)
				{
					await AuthHelper.ReloginAsync();
					return;
				}

				this.Data = (await App.Database.GetList(new PersonType[] { PersonType.Alive, PersonType.Dead }))
					.Where(i => i.Owner == AuthHelper.CurrentUserId.Value)
					.Select(i => new ListElement.ImageElement
					{
						FIO = i.FIO,
						Image = i.SmallImageUrl == null ? ImageSource.FromFile("no_img.png") : ImageSource.FromUri(i.SmallImageUrl),
						Person = i
					}).ToList();

			}

		}

    }
}
