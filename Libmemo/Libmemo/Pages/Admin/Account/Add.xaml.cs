using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo.Pages.Admin.Account
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Add : ContentPage
    {
		private ViewModel Model
		{
			get => (ViewModel)BindingContext;
			set => BindingContext = value;
		}

		public Add()
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


			private string _email;
			public string Email
			{
				get => this._email;
				set
				{
					if (this._email != value)
					{
						this._email = value;
						this.OnPropertyChanged(nameof(Email));
					}
				}
			}

			private string _password;
			public string Password
			{
				get => this._password;
				set
				{
					if (this._password != value)
					{
						this._password = value;
						this.OnPropertyChanged(nameof(Password));
					}
				}
			}

			private string _confirmPassword;
			public string ConfirmPassword
			{
				get => this._confirmPassword;
				set
				{
					if (this._confirmPassword != value)
					{
						this._confirmPassword = value;
						this.OnPropertyChanged(nameof(ConfirmPassword));
					}
				}
			}

			protected virtual IEnumerable<string> Validate()
			{
				if (string.IsNullOrWhiteSpace(this.Email) || !Regex.IsMatch(this.Email,
					@"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
					@"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
					RegexOptions.IgnoreCase))
				{
					yield return "Невалидный email адрес";
				}

				if (string.IsNullOrWhiteSpace(this.Password))
				{
					yield return "Не заполнен пароль";
				}
				if (string.IsNullOrWhiteSpace(this.ConfirmPassword))
				{
					yield return "Не заполнено подтверждение пароля";
				}
				if (!string.Equals(this.Password, this.ConfirmPassword))
				{
					yield return "Пароли не совпадают";
				}
			}


			public ICommand RegisterCommand => new Command(async () => {
				if (cancelTokenSource != null) return;


				if (string.IsNullOrWhiteSpace(this.Email) || !Regex.IsMatch(this.Email,
					@"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
					@"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
					RegexOptions.IgnoreCase)
				)
				{
					App.ToastNotificator.Show("Невалидный email адрес");
					return;
				}

				if (string.IsNullOrWhiteSpace(this.Password))
				{
					App.ToastNotificator.Show("Не заполнен пароль");
					return;
				}
				if (string.IsNullOrWhiteSpace(this.ConfirmPassword))
				{
					App.ToastNotificator.Show("Не заполнено подтверждение пароля");
					return;
				}
				if (!string.Equals(this.Password, this.ConfirmPassword))
				{
					App.ToastNotificator.Show("Пароли не совпадают");
					return;
				}


				StartLoading("Регистрация");


				var content = new FormUrlEncodedContent(new Dictionary<string, string> {
					{"email", this.Email },
					{"password", this.Password },
					{"confirm", this.ConfirmPassword }
				});

				HttpResponseMessage response;
				try
				{
					cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Post, new Uri(Settings.ADMIN_USERS_URL), content, 60, cancelTokenSource.Token);
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
					if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
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

				Json.Auth data;
				try
				{
					data = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Auth>(str);
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка ответа сервера");
					return;
				}




                App.ToastNotificator.Show("Завершено");
				await App.GlobalPage.Pop();
			});

		}
    }
}
