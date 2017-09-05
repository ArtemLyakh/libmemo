using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Windows.Input;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace Libmemo.Pages.Admin.Account
{
    public partial class Account : ContentPage
    {
		private ViewModel Model
		{
			get => (ViewModel)BindingContext;
			set => BindingContext = value;
		}

		public Account(int id)
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

            private string _email;
            public string Email {
                get => _email;
                set {
                    if (_email != value) {
                        _email = value;
                        OnPropertyChanged(nameof(Email));
                    }
                }
            }

			private string _firstName;
			public string FirstName
			{
				get => _firstName;
				set
				{
					if (_firstName != value)
					{
						_firstName = value;
						this.OnPropertyChanged(nameof(FirstName));
					}
				}
			}

			private string _secondName;
			public string SecondName
			{
				get => _secondName;
				set
				{
					if (_secondName != value)
					{
						_secondName = value;
						this.OnPropertyChanged(nameof(SecondName));
					}
				}
			}

			private string _lastName;
			public string LastName
			{
				get => _lastName;
				set
				{
					if (_lastName != value)
					{
						_lastName = value;
						this.OnPropertyChanged(nameof(LastName));
					}
				}
			}

			private DateTime? _dateBirth = null;
			public DateTime? DateBirth
			{
				get => _dateBirth;
				set
				{
					if (_dateBirth != value)
					{
						_dateBirth = value;
						this.OnPropertyChanged(nameof(DateBirth));
					}
				}
			}

			private ImageSource _photoSource;
			public ImageSource PhotoSource
			{
				get => _photoSource;
				set
				{
					if (_photoSource != value)
					{
						_photoSource = value;
						this.OnPropertyChanged(nameof(PhotoSource));
					}
				}
			}


			public ICommand PickPhotoCommand => new Command(async () => {
				if (CrossMedia.Current.IsPickPhotoSupported)
				{
					var photo = await CrossMedia.Current.PickPhotoAsync();
					if (photo == null) return;
					this.PhotoSource = ImageSource.FromFile(photo.Path);
				}
				else
				{
					App.ToastNotificator.Show("Выбор фото невозможен");
				}
			});
			public ICommand MakePhotoCommand => new Command(async () => {
				if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported)
				{
					var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { SaveToAlbum = false });
					if (file == null) return;
					PhotoSource = ImageSource.FromFile(file.Path);
				}
				else
				{
					App.ToastNotificator.Show("Сделать фото невозможно");
				}
			});


            private void SetData(Json.Admin.AccountData data)
			{
				this.Email = data.email;
                this.FirstName = data.first_name;
                this.SecondName = data.second_name;
                this.LastName = data.last_name;
                this.DateBirth = DateTime.TryParse(data.date_birth, out DateTime dateBirth)
                    ? (DateTime?)dateBirth
                    : null;
                this.PhotoSource = Uri.TryCreate(data.photo_url, UriKind.Absolute, out Uri photoUri)
                    ? ImageSource.FromUri(photoUri)
                    : null;
			}

			public ICommand ResetCommand => new Command(async () => {
				if (cancelTokenSource != null) return;

				StartLoading("Получение данных");

				HttpResponseMessage response;
				try
				{
					cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Get, new Uri($"{Settings.ADMIN_USERS_URL}{Id}/"), null, 30, cancelTokenSource.Token);
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


                Json.Admin.AccountData data;
				try
				{
                    data = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Admin.AccountData>(str);
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка ответа сервера");
					return;
				}

				SetData(data);
			});

			public ICommand SaveCommand => new Command(async () => {
				if (cancelTokenSource != null) return;


				if (string.IsNullOrWhiteSpace(this.FirstName))
				{
					App.ToastNotificator.Show("Имя не может быть пустым");
					return;
				}


				StartLoading("Сохранение");


				var content = new MultipartFormDataContent(String.Format("----------{0:N}", Guid.NewGuid()));
				content.Add(new StringContent(this.FirstName), "first_name");
				content.Add(new StringContent(this.SecondName ?? string.Empty), "second_name");
				content.Add(new StringContent(this.LastName ?? string.Empty), "last_name");
				if (this.DateBirth.HasValue)
				{
					content.Add(new StringContent(this.DateBirth.Value.ToString("yyyy-MM-dd")), "date_birth");
				}
				if (this.PhotoSource != null && this.PhotoSource is FileImageSource)
				{
					var result = await DependencyService.Get<IFileStreamPicker>().GetResizedJpegAsync((PhotoSource as FileImageSource).File, 1000, 1000);
					content.Add(new ByteArrayContent(result), "photo", "photo.jpg");
				}


				HttpResponseMessage response;
				try
				{
					cancelTokenSource = new CancellationTokenSource();
					response = await WebClient.Instance.SendAsync(HttpMethod.Post, new Uri($"{Settings.ADMIN_USERS_URL}{Id}/"), content, 60, cancelTokenSource.Token);
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


                Json.Admin.AccountData data;
				try
				{
                    data = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Admin.AccountData>(str);
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка ответа сервера");
					return;
				}

				SetData(data);
			});

		}
    }
}
