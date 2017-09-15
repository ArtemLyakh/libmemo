using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Detail : ContentPage
    {
		private ViewModel Model
		{
			get => (ViewModel)BindingContext;
			set => BindingContext = value;
		}

		public Detail(int id)
		{
			InitializeComponent();
			BindingContext = new ViewModel(id);
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
			private int Id { get; set; }

			public ViewModel(int id) : base()
			{
				Id = id;
				ResetCommand.Execute(null);
			}


            private string _fio;
            public string Fio {
                get => _fio;
                set {
                    if (_fio != value) {
                        _fio = value;
                        OnPropertyChanged(nameof(Fio));
                    }
                }
            }

            private string _coordinates;
            public string Coordinates {
                get => _coordinates;
                set {
                    if (_coordinates != value) {
                        _coordinates = value;
                        OnPropertyChanged(nameof(Coordinates));
                    }
                }
            }

            public bool IsDateLiveShow => !string.IsNullOrWhiteSpace(DateLive);
            private string _dateLive;
            public string DateLive {
                get => _dateLive;
                set {
                    if (_dateLive != value) {
                        _dateLive = value;
                        OnPropertyChanged(nameof(DateLive));
                        OnPropertyChanged(nameof(IsDateLiveShow));
                    }
                }
            }

			private List<ImageSource> _photos = new List<ImageSource>();
			private List<ImageSource> Photos
			{
				get => _photos;
				set
				{
					if (_photos != value)
					{
						_photos = value;
						OnPropertyChanged(nameof(ImageCollection));
					}
				}
			}
			public class Image
			{
				public ImageSource PhotoSource { get; set; }
				public ICommand PickPhotoCommand { get; set; }
				public ICommand MakePhotoCommand { get; set; }
			}
			public List<Image> ImageCollection => Photos.Select(i => new Image
			{
				PhotoSource = i
			}).ToList();



			private string _text;
			public string Text {
				get => _text; 
				set {
					if (_text != value) {
						this._text = value;
						this.OnPropertyChanged(nameof(Text));
					}
				}
			}



            public bool IsHeightShow => !string.IsNullOrWhiteSpace(Height);
			private string _height;
			public string Height
			{
				get => _height; 
				set {
					if (_height != value) {
						_height = value;
						OnPropertyChanged(nameof(Height));
                        OnPropertyChanged(nameof(IsHeightShow));
					}
				}
			}


            public bool IsWidthShow => !string.IsNullOrWhiteSpace(Width);
			private string _width;
			public string Width
			{
				get => _width;
				set {
					if (_width != value) {
						_width = value;
						OnPropertyChanged(nameof(Width));
                        OnPropertyChanged(nameof(IsWidthShow));
					}
				}
			}


            public bool IsSchemeShow => SchemeUri != null;
            private Uri _schemeUri;
			private Uri SchemeUri { 
                get => _schemeUri;
                set {
                    if (_schemeUri != value) {
                        _schemeUri = value;
                        OnPropertyChanged(nameof(IsSchemeShow));
                    }
                }
            }
			public ICommand SchemeDownloadCommand => new Command(() => {
				if (this.SchemeUri != null) Device.OpenUri(SchemeUri);
			});


			public bool IsSectionShow => !string.IsNullOrWhiteSpace(Section);
			private string _section;
			public string Section
			{
				get => _section;
				set
				{
					if (_section != value)
					{
						_section = value;
						OnPropertyChanged(nameof(Section));
                        OnPropertyChanged(nameof(IsSectionShow));
					}
				}
			}


            public bool IsGraveNumberShow => !string.IsNullOrWhiteSpace(GraveNumber);
			private string _graveNumber;
			public string GraveNumber
			{
				get => _graveNumber;
				set
				{
					if (_graveNumber != value)
					{
						_graveNumber = value;
						OnPropertyChanged(nameof(GraveNumber));
                        OnPropertyChanged(nameof(IsGraveNumberShow));
					}
				}
			}


            private List<Json.PersonDetail.Tree> _trees;
            private List<Json.PersonDetail.Tree> Trees {
                get => _trees;
                set {
                    if (_trees != value) {
                        _trees = value;
                        OnPropertyChanged(nameof(IsTreeButtonShow));
                    }
                }
            }
            public bool IsTreeButtonShow => Trees != null && Trees.Count > 0;

            public ICommand TreeShowCommand => new Command(async () => {
                if (Trees == null || Trees.Count == 0) return;
                await App.GlobalPage.Push(new Pages.TreeList(Trees));
            });


			public ICommand ResetCommand => new Command(async () => {
				if (cancelTokenSource != null) return;

				StartLoading("Получение данных");

				HttpResponseMessage response;
				try
				{
					cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Get, new Uri($"{Settings.PERSONS_URL}{Id}/"), null, 30, cancelTokenSource.Token);
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
					if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
					{
						throw new UnauthorizedAccessException();
					}
					if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
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


				Models.Person person;
                List<Json.PersonDetail.Tree> trees;
				try
				{
                    var json = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.PersonDetail>(str);
                    person = Models.Person.Convert(json.person);
                    trees = json.trees;
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка ответа сервера");
					return;
				}

				SetData(person, trees);
			});



            private void SetData(Models.Person person, List<Json.PersonDetail.Tree> trees)
            {
                var fio = new List<string>();
                if (!string.IsNullOrWhiteSpace(person.LastName)) fio.Add(person.LastName);
                fio.Add(person.FirstName);
                if (!string.IsNullOrWhiteSpace(person.SecondName)) fio.Add(person.SecondName);
                this.Fio = string.Join(" ", fio);

                Photos = person.Images.Select(i => ImageSource.FromUri(i.Value)).ToList();


				if (person is Models.DeadPerson)
				{
					var deadPerson = (Models.DeadPerson)person;

                    this.Coordinates = $"{deadPerson.Latitude.ToString(CultureInfo.InvariantCulture)}:{deadPerson.Longitude.ToString(CultureInfo.InvariantCulture)}";

                    if (deadPerson.DateBirth.HasValue && deadPerson.DateDeath.HasValue) {
                        this.DateLive = $"{deadPerson.DateBirth.Value.ToString("dd.MM.yyyy")} - {deadPerson.DateDeath.Value.ToString("dd.MM.yyyy")}";
                    }

                    this.Text = deadPerson.Text;
                    this.Width = deadPerson.Width?.ToString();
                    this.Height = deadPerson.Height?.ToString();
                    this.Section = deadPerson.Section;
                    this.GraveNumber = deadPerson.GraveNumber?.ToString();

                    this.SchemeUri = deadPerson.Scheme;
				}


                this.Trees = trees;
			}

		}
    }
}
