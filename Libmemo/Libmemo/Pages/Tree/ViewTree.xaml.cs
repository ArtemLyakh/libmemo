using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewTree : ContentPage
    {
		private ViewModel Model
		{
			get => (ViewModel)BindingContext;
			set => BindingContext = value;
		}

		public ViewTree(int id)
		{
			InitializeComponent();
			BindingContext = new ViewModel(absolute, scroll, id);
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
			private Helpers.Tree Tree { get; set; }
            private int Id { get; set; }

			public ViewModel(AbsoluteLayout absolute, ScrollView scroll, int id) : base()
			{
                Id = id;
                Tree = new Helpers.Tree(absolute, scroll, true);
				ResetCommand.Execute(null);
			}

			public ICommand ResetCommand => new Command(async () => {
				if (cancelTokenSource != null) return;


				StartLoading("Получение данных");


				HttpResponseMessage response;
				try
				{
					cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Get, new Uri($"{Settings.TREE_URL}{Id}/"), null, 30, cancelTokenSource.Token);
				}
				catch (TimeoutException)
				{
					App.ToastNotificator.Show("Превышен интервал запроса");
					return;
				}
				catch (OperationCanceledException)
				{ //cancel
					return;
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка");
					return;
				}
				finally
				{
					cancelTokenSource = null;
					StopLoading();
				}



				var str = await response.Content.ReadAsStringAsync();



				try
				{
					if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
					{
						throw new UnauthorizedAccessException();
					}
					if (response.StatusCode == System.Net.HttpStatusCode.BadRequest
						|| response.StatusCode == System.Net.HttpStatusCode.InternalServerError
					)
					{
						var msg = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Error>(str);
						throw new HttpRequestException(msg.error);
					}
					response.EnsureSuccessStatusCode();
				}
				catch (UnauthorizedAccessException)
				{
					await AuthHelper.ReloginAsync();
					return;
				}
				catch (HttpRequestException ex)
				{
					App.ToastNotificator.Show(ex.Message);
					return;
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка");
					return;
				}


				Json.Tree json;
				try
				{
					json = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Tree>(str);
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка ответа сервера");
					return;
				}

				try
				{
					await Tree.DrawTree(json);
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка построения");
				}

			});




			public ICommand ZoomInCommand => new Command(async () => await Tree?.ZoomIn());
			public ICommand ZoomOutCommand => new Command(async () => await Tree?.ZoomOut());

			

		}
    }
}
