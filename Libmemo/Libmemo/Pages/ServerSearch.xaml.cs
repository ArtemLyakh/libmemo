using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ServerSearch : ContentPage
    {
		private ViewModel Model
		{
			get => (ViewModel)BindingContext;
			set => BindingContext = value;
		}

        public ServerSearch(Action<Json.UserListEntry> cb)
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
            private Action<Json.UserListEntry> Callback { get; set; }

            public ViewModel(Action<Json.UserListEntry> cb) : base() {
                Callback = cb;
            }

            public override void OnAppearing()
            {
                base.OnAppearing();
                SearchCommand.Execute(null);
            }

			public class Entry
			{
				public int Id { get; set; }
				public bool IsAlive { get; set; }
				public string Text { get; set; }
				public ImageSource Image { get; set; }

				public Entry(int id, string text)
				{
					Id = id;
					Text = text;
				}
			}

            private Dictionary<int, Json.UserListEntry> RawJsonData;

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

			private string _search;
			public string Search
			{
				get => _search;
				set
				{
					if (_search != value)
					{
						_search = value;
						OnPropertyChanged(nameof(Search));
					}
				}
			}



            public ICommand SearchCommand => new Command(async () => {
                if (cancelTokenSource != null) return;


                if (string.IsNullOrWhiteSpace(Search)) return;


                StartLoading("Получение данных");


                var uri = new UriBuilder(Settings.PERSONS_URL) { Query = $"search={Search}" }.Uri;


				HttpResponseMessage response;
				try
				{
					cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Get, uri, null, 30, cancelTokenSource.Token);
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


				var str = await response.Content.ReadAsStringAsync();


				try
				{
					if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
					{
						var msg = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Error>(str);
						throw new HttpRequestException(msg.error);
					}
					response.EnsureSuccessStatusCode();
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


				List<Json.UserListEntry> data;
				try
				{
					data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Json.UserListEntry>>(str);
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка ответа сервера");
					return;
				}

                RawJsonData = new Dictionary<int, Json.UserListEntry>();
                foreach (var entry in data) {
                    RawJsonData[entry.id] = entry;
                }

				var listData = new List<Entry>();
				foreach (var entry in data)
				{
					if (string.IsNullOrWhiteSpace(entry.first_name)) continue;

					var fio = new List<string>();
					if (!string.IsNullOrWhiteSpace(entry.last_name)) fio.Add(entry.last_name);
					fio.Add(entry.first_name);
					if (!string.IsNullOrWhiteSpace(entry.second_name)) fio.Add(entry.second_name);

					listData.Add(new Entry(entry.id, string.Join(" ", fio))
					{
						IsAlive = entry.type == "alive" || entry.type == "user",
						Image = (!string.IsNullOrWhiteSpace(entry.preview_image_url)
								&& Uri.TryCreate(entry.preview_image_url, UriKind.Absolute, out Uri image)
							) ? ImageSource.FromUri(image)
							: ImageSource.FromFile("no_img.png")
					});
				}

				this.Data = listData;
            });





			public ICommand ItemSelectedCommand => new Command<object>(selected => {
				var entry = (Entry)selected;
                var json = RawJsonData[entry.Id];
                Callback?.Invoke(json);
			});
		}
    }
}
