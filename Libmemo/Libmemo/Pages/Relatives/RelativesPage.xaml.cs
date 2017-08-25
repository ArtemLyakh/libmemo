using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RelativesPage : ContentPage
    {
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

			public ICommand AddCommand => new Command(async () => await App.GlobalPage.Push(new AddPage()));

            public ICommand ResetCommand => new Command(async () =>
            {
				if (cancelTokenSource != null) return;

				StartLoading("Получение данных");

				HttpResponseMessage response = null;
				try {
					cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Get, new Uri(Settings.RELATIVES_URL), null, 15, cancelTokenSource.Token);
				} catch (TimeoutException) {
					App.ToastNotificator.Show("Превышен интервал запроса");
					return;
				} catch (OperationCanceledException) { //cancel
					return;
				} catch {
					App.ToastNotificator.Show("Ошибка");
					return;
				} finally {
					cancelTokenSource = null;
					StopLoading();
				}
				if (response == null) return;

				try {
					if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
						throw new UnauthorizedAccessException();
					}
					response.EnsureSuccessStatusCode();
				} catch (UnauthorizedAccessException) {
					await AuthHelper.ReloginAsync();
					return;
				} catch {
					App.ToastNotificator.Show("Ошибка");
					return;
				}

				var str = await response.Content.ReadAsStringAsync();
				var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Json.User>>(str);

				this.Data = data.Select(i => new ListElement.TextElement { Id = i.id, Fio = i.fio, Email = i.email }).ToList();
            });

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
