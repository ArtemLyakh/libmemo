using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo.Pages
{
    public partial class Tree : ContentPage
    {
		private ViewModel Model
		{
			get => (ViewModel)BindingContext;
			set => BindingContext = value;
		}

		public Tree()
		{
			InitializeComponent();
			BindingContext = new ViewModel(absolute, scroll);
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

			public ViewModel(AbsoluteLayout absolute, ScrollView scroll) : base()
			{
                Tree = new Helpers.Tree(absolute, scroll);
				ResetCommand.Execute(null);
			}

			public ICommand ResetCommand => new Command(async () => {
				if (cancelTokenSource != null) return;


				StartLoading("Получение данных");


				HttpResponseMessage response;
				try
				{
					cancelTokenSource = new CancellationTokenSource();
					response = await WebClient.Instance.SendAsync(HttpMethod.Get, new Uri(Settings.TREE_URL), null, 30, cancelTokenSource.Token);
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

                try {
                    await Tree.DrawTree(json);
                } catch {
                    App.ToastNotificator.Show("Ошибка построения");
                }

			});




			public ICommand ZoomInCommand => new Command(async () => await Tree?.ZoomIn());
			public ICommand ZoomOutCommand => new Command(async () => await Tree?.ZoomOut());


			public ICommand SaveCommand => new Command(async () => { 
                if (cancelTokenSource != null) return;

                List<Json.TreeSave> list;
                try {
                    list = Tree.GetTreeAsJson();
                } catch {
                    App.ToastNotificator.Show("Древо не инициализировано");
                    return;
                }

				var json = Newtonsoft.Json.JsonConvert.SerializeObject(list);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

                StartLoading("Сохранение");

				HttpResponseMessage response;
				try
				{
					cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Post, new Uri(Settings.TREE_URL), content, 60, cancelTokenSource.Token);
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
					response.EnsureSuccessStatusCode();
				}
				catch (UnauthorizedAccessException)
				{
					await AuthHelper.ReloginAsync();
					return;
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка");
					return;
				}



                App.ToastNotificator.Show("Сохранено");

			});


		}
    }
}
