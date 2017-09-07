using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo.Pages.Admin.Relatives
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserSelect : ContentPage
    {
		private ViewModel Model
		{
			get => (ViewModel)BindingContext;
			set => BindingContext = value;
		}

		public UserSelect(Action<Json.Admin.User> cb)
		{
			InitializeComponent();
			BindingContext = new ViewModel(cb);
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
			private Action<Json.Admin.User> CallBack;

			public ViewModel(Action<Json.Admin.User> cb) : base()
			{
				CallBack = cb;
			}

			public override void OnAppearing()
			{
				base.OnAppearing();
				ResetCommand.Execute(null);
			}

			public class Entry
			{
				public int Id { get; set; }
				public string Text { get; set; }

				public Entry(int id, string text)
				{
					Id = id;
					Text = text;
				}
			}

			private List<Json.Admin.User> FullData { get; set; }

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

			private string _filter;
			public string Filter
			{
				get => _filter;
				set
				{
					if (_filter != value)
					{
						_filter = value;
						OnPropertyChanged(nameof(Filter));
					}
				}
			}

			public ICommand ResetCommand => new Command(async () =>
			{
				if (cancelTokenSource != null) return;

				StartLoading("Получение данных");

				HttpResponseMessage response;
				try
				{
					cancelTokenSource = new CancellationTokenSource();
					response = await WebClient.Instance.SendAsync(HttpMethod.Get, new Uri(Settings.ADMIN_USERS_URL), null, 30, cancelTokenSource.Token);
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
					App.ToastNotificator.Show("Ошибка запроса");
					return;
				}
				finally
				{
					cancelTokenSource = null;
					StopLoading();
				}

				try
				{
					if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
						throw new UnauthorizedAccessException();
					response.EnsureSuccessStatusCode();
				}
				catch (UnauthorizedAccessException)
				{
					await AuthHelper.ReloginAsync();
					return;
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка сервера");
					return;
				}

				var str = await response.Content.ReadAsStringAsync();


				List<Json.Admin.User> data;
				try
				{
					data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Json.Admin.User>>(str);
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка ответа сервера");
					return;
				}


                this.FullData = data;
				this.Filter = string.Empty;

				FilterCommand.Execute(null);
			});

			public ICommand FilterCommand => new Command(() => {
				if (string.IsNullOrWhiteSpace(this.Filter))
				{
					this.Data = this.FullData.Select(i => new Entry(i.id, $"{i.id}: {i.fio}")).ToList();
				}
				else
				{
					this.Data = FullData.Select(i => new Entry(i.id, $"{i.id}: {i.fio}"))
						.DefaultIfEmpty()
						.Where(i => i.Text.ToLowerInvariant().IndexOf(this.Filter.ToLowerInvariant()) != -1)
						.Select(i => i)
						.ToList();
				}
			});

			public ICommand ItemSelectedCommand => new Command<object>(selected => {
				var entry = (Entry)selected;
                CallBack.Invoke(FullData.First(i => i.id == entry.Id));
			});
		}
    }
}
