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

namespace Libmemo.Pages {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Relatives : ContentPage
    {
		private ViewModel Model
		{
			get => (ViewModel)BindingContext;
			set => BindingContext = value;
		}

		public Relatives()
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
			public ViewModel() : base() { }

            public override void OnAppearing()
            {
                base.OnAppearing();
                ResetCommand.Execute(null);
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

            private List<Entry> FullData { get; set; }

            private List<Entry> _data;
            public List<Entry> Data {
                get => _data;
                set {
                    _data = value;
                    OnPropertyChanged(nameof(Data));
                }
            }

            private string _filter;
            public string Filter { 
                get => _filter;
                set {
                    if (_filter != value) {
                        _filter = value;
                        OnPropertyChanged(nameof(Filter));
                    }
                }
            }



            public ICommand AddCommand => new Command(async () => await App.GlobalPage.Push(new AddRelative()));

            public class Json
            {
                public class Entry
                {
                    public int? id { get; set; }
                    public string type { get; set; }
                    public string first_name { get; set; }
                    public string second_name { get; set; }
                    public string last_name { get; set; }
                    public string preview_image_url { get; set; }
                }
                public List<Entry> relatives { get; set; }
            }
            
            public ICommand ResetCommand => new Command(async () =>
            {
				if (cancelTokenSource != null) return;

				StartLoading("Получение данных");

                HttpResponseMessage response;
				try {
					cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Get, new Uri(Settings.RELATIVES_URL), null, 20, cancelTokenSource.Token);
				} catch (TimeoutException) {
					App.ToastNotificator.Show("Превышен интервал запроса");
					return;
				} catch (OperationCanceledException) { //cancel
					return;
				} catch {
					App.ToastNotificator.Show("Ошибка запроса");
					return;
				} finally {
					cancelTokenSource = null;
					StopLoading();
				}

				try {
					if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) 
						throw new UnauthorizedAccessException();
					response.EnsureSuccessStatusCode();
				} catch (UnauthorizedAccessException) {
					await AuthHelper.ReloginAsync();
					return;
				} catch {
					App.ToastNotificator.Show("Ошибка сервера");
					return;
				}

				var str = await response.Content.ReadAsStringAsync();
                Json data;
                try {
                    data = Newtonsoft.Json.JsonConvert.DeserializeObject<Json>(str);
                } catch {
                    App.ToastNotificator.Show("Ошибка ответа сервера");
                    return;
                }

                var listData = new List<Entry>();
                foreach(var entry in data.relatives) {
                    if (!entry.id.HasValue) continue;
                    if (string.IsNullOrWhiteSpace(entry.first_name)) continue;

                    var fio = new List<string>();
                    if (!string.IsNullOrWhiteSpace(entry.last_name)) fio.Add(entry.last_name);
                    fio.Add(entry.first_name);
                    if (!string.IsNullOrWhiteSpace(entry.second_name)) fio.Add(entry.second_name);

                    listData.Add(new Entry(entry.id.Value, string.Join(" ", fio)) {
                        IsAlive = entry.type == "alive",
                        Image = (!string.IsNullOrWhiteSpace(entry.preview_image_url)
                                && Uri.TryCreate(entry.preview_image_url, UriKind.Absolute, out Uri image)
                            ) ? ImageSource.FromUri(image)
                            : ImageSource.FromFile("no_img.png")
                    });
                }

                this.FullData = listData;
                this.Filter = string.Empty;

                FilterCommand.Execute(null);
            });

            public ICommand FilterCommand => new Command(() => {
                if (string.IsNullOrWhiteSpace(this.Filter)) {
                    this.Data = this.FullData;
                } else {
                    this.Data = FullData
                        .DefaultIfEmpty()
                        .Where(i => i.Text.ToLowerInvariant().IndexOf(this.Filter.ToLowerInvariant()) != -1)
                        .Select(i => i)
                        .ToList();
                }
            });

            public ICommand ItemSelectedCommand => new Command<object>(async selected => {
                var entry = (Entry)selected;
                await App.GlobalPage.Push(new EditRelative(entry.Id));
            });
        }

    }
}
