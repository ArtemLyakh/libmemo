using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Libmemo.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditRelative : ContentPage
    {
        private ViewModel Model {
            get => (ViewModel)BindingContext;
            set => BindingContext = value;
        }

        public EditRelative(int id)
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

                Pin = new CustomPin() {
                    PinImage = PinImage.Default,
                    Id = id.ToString(),
                    Position = new Position(),
                    Visible = true
                };
                CustomPins.Add(Pin);

                PersonTypeChanged += (sender, type) => this.IsDeadPerson = type == Models.PersonType.Dead;
                UserPositionChanged += (sender, position) => this.UserPosition = position;


                ResetCommand.Execute(null);
            }


            public override void OnAppearing()
            {
                base.OnAppearing();
                MyLocationEnabled = true;
            }

            public override void OnDisappearing()
            {
                base.OnDisappearing();
                MyLocationEnabled = false;
            }

            #region Person type

            private Dictionary<Models.PersonType, string> personTypeDictionary { get; } = new Dictionary<Models.PersonType, string> {
                { Models.PersonType.Dead, "Мертвый" },
                { Models.PersonType.Alive, "Живой" }
            };
            public List<string> PersonTypeList =>
                personTypeDictionary.Select(i => i.Value).ToList();

            private Models.PersonType Type {
                get => personTypeDictionary.ElementAt(PersonTypeIndex).Key;
                set => PersonTypeIndex = PersonTypeList.IndexOf(personTypeDictionary[value]);
            }

            private int _personTypeIndex;
            public int PersonTypeIndex {
                get => _personTypeIndex;
                set {
                    _personTypeIndex = value;
                    OnPropertyChanged(nameof(PersonTypeIndex));
                    PersonTypeChanged?.Invoke(this, Type);
                }
            }

            private event EventHandler<Models.PersonType> PersonTypeChanged;

            #endregion

            #region Map

            private ObservableCollection<CustomPin> _customPins = new ObservableCollection<CustomPin>();
            public ObservableCollection<CustomPin> CustomPins => _customPins;

            private CustomPin Pin { get; set; }
            private Position? _personPosition;
            public Position? PersonPosition {
                get => _personPosition;
                set {
                    _personPosition = value;
                    if (Pin != null) Pin.Position = value ?? default(Position);
                    OnPropertyChanged(nameof(Coordinates));
                }
            }
            public string Coordinates => PersonPosition.HasValue 
                ? $"{PersonPosition.Value.Latitude}\n{PersonPosition.Value.Longitude}" 
                : $"{char.ConvertFromUtf32(0x2014)}\n{char.ConvertFromUtf32(0x2014)}";


            private event EventHandler<Position> PersonPositionChanged;
            public ICommand MapClickCommand => new Command<Position>(position => {
                PersonPosition = position;
                PersonPositionChanged?.Invoke(this, position);
            });

           

            private bool _isMapVisible = false;
            public bool IsMapVisible {
                get => _isMapVisible;
                set {
                    _isMapVisible = value;
                    OnPropertyChanged(nameof(IsMapVisible));
                }
            }

            public ICommand HideMap => new Command(() => IsMapVisible = false);
            public ICommand ShowMap => new Command(() => IsMapVisible = true);

            private Position _mapCenter;
            public Position MapCenter {
                get => _mapCenter;
                set {
                    if (_mapCenter != value) {
                        _mapCenter = value;
                        this.OnPropertyChanged(nameof(MapCenter));
                    }
                }
            }

            private float _zoom = 18;
            public float Zoom {
                get => _zoom;
                set {
                    if (_zoom != value) {
                        _zoom = value;
                        this.OnPropertyChanged(nameof(Zoom));
                    }
                }
            }

            private bool _myLocationEnabled = false;
            public bool MyLocationEnabled {
                get => _myLocationEnabled;
                private set {
                    _myLocationEnabled = value;
                    this.OnPropertyChanged(nameof(MyLocationEnabled));
                }
            }

            public ICommand UserPositionChangedCommand => new Command<Position>(position => UserPositionChanged?.Invoke(this, position));
            private event EventHandler<Position> UserPositionChanged;


            private Position? _userPosition = null;
            public Position? UserPosition {
                get { return this._userPosition; }
                private set {
                    if (this._userPosition != value) {
                        this._userPosition = value;
                        this.OnPropertyChanged(nameof(UserPosition));
                        this.OnPropertyChanged(nameof(Latitude));
                        this.OnPropertyChanged(nameof(Longitude));
                    }
                }
            }
            public string Latitude => this.UserPosition?.Latitude.ToString() ?? char.ConvertFromUtf32(0x2014);
            public string Longitude => this.UserPosition?.Longitude.ToString() ?? char.ConvertFromUtf32(0x2014);

            #endregion

            private bool _isDeadPerson = true;
            public bool IsDeadPerson {
                get => _isDeadPerson;
                set {
                    _isDeadPerson = value;
                    OnPropertyChanged(nameof(IsDeadPerson));
                }
            }


            private string _firstName;
            public string FirstName {
                get => _firstName;
                set {
                    if (_firstName != value) {
                        _firstName = value;
                        this.OnPropertyChanged(nameof(FirstName));
                    }
                }
            }

            private string _secondName;
            public string SecondName {
                get => _secondName;
                set {
                    if (_secondName != value) {
                        _secondName = value;
                        this.OnPropertyChanged(nameof(SecondName));
                    }
                }
            }

            private string _lastName;
            public string LastName {
                get => _lastName;
                set {
                    if (_lastName != value) {
                        _lastName = value;
                        this.OnPropertyChanged(nameof(LastName));
                    }
                }
            }

            private DateTime? _dateBirth = null;
            public DateTime? DateBirth {
                get => _dateBirth;
                set {
                    if (_dateBirth != value) {
                        _dateBirth = value;
                        this.OnPropertyChanged(nameof(DateBirth));
                    }
                }
            }

            private DateTime? _dateDeath = null;
            public DateTime? DateDeath {
                get => _dateDeath;
                set {
                    if (_dateDeath != value) {
                        _dateDeath = value;
                        this.OnPropertyChanged(nameof(DateDeath));
                    }
                }
            }

            private string _text;
            public string Text {
                get => _text;
                set {
                    if (_text != value) {
                        _text = value;
                        this.OnPropertyChanged(nameof(Text));
                    }
                }
            }

            private ImageSource _photoSource;
            public ImageSource PhotoSource {
                get => _photoSource;
                set {
                    if (_photoSource != value) {
                        _photoSource = value;
                        this.OnPropertyChanged(nameof(PhotoSource));
                    }
                }
            }

            public ICommand PickPhotoCommand => new Command(async () => {
                if (CrossMedia.Current.IsPickPhotoSupported) {
                    var photo = await CrossMedia.Current.PickPhotoAsync();
                    if (photo == null) return;
                    this.PhotoSource = ImageSource.FromFile(photo.Path);
                } else {
                    App.ToastNotificator.Show("Выбор фото невозможен");
                }
            });
            public ICommand MakePhotoCommand => new Command(async () => {
                if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported) {
                    var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { SaveToAlbum = false });
                    if (file == null) return;
                    PhotoSource = ImageSource.FromFile(file.Path);
                } else {
                    App.ToastNotificator.Show("Сделать фото невозможно");
                }
            });


            private double? _height;
            public double? Height {
                get => _height;
                set {
                    if (_height != value) {
                        _height = value;
                        this.OnPropertyChanged(nameof(Height));
                    }
                }
            }
            private double? _width;
            public double? Width {
                get => _width;
                set {
                    if (_width != value) {
                        _width = value;
                        this.OnPropertyChanged(nameof(Width));
                    }
                }
            }

            private Stream SchemeStream { get; set; }
            private string _schemeName;
            public string SchemeName {
                get => string.IsNullOrWhiteSpace(_schemeName) ? "Не выбрано" : _schemeName;
                private set {
                    if (_schemeName != value) {
                        _schemeName = value;
                        OnPropertyChanged(nameof(SchemeName));
                    }
                }
            }
            private void SetScheme(string name, Stream stream)
            {
                SchemeUrl = null;
                ResetScheme();

                SchemeName = name;
                SchemeStream = stream;
            }
            private void ResetScheme()
            {
                SchemeName = null;

                SchemeStream?.Dispose();
                SchemeStream = null;
            }

            public ICommand SelectSchemeCommand => new Command(async () => {
                var file = await Plugin.FilePicker.CrossFilePicker.Current.PickFile();
                if (file == null) return;

                Stream stream;
                try {
                    stream = DependencyService.Get<IFileStreamPicker>().GetStream(file.FilePath);
                } catch {
                    App.ToastNotificator.Show("Ошибка выбора файла");
                    return;
                }

                if (stream.Length > 2 * 1024 * 1024) {
                    App.ToastNotificator.Show($"Размер файла не должен превышать 2 МБ ({stream.Length / 1024 / 1024} МБ)");
                    return;
                }
                SetScheme(file.FileName, stream);
            });

            private Uri _schemeUrl = null;
            protected Uri SchemeUrl {
                get => _schemeUrl;
                set {
                    if (this._schemeUrl != value) {
                        this._schemeUrl = value;
                        this.OnPropertyChanged(nameof(IsSchemeCanDownload));
                    }
                }
            }
            public bool IsSchemeCanDownload => SchemeUrl != null;
            public ICommand SchemeDownloadCommand => new Command(() => {
                if (SchemeUrl != null) Device.OpenUri(SchemeUrl);
            });



            private void SetData(Models.Person person)
            {
                this.FirstName = person.FirstName;
                this.LastName = person.LastName;
                this.SecondName = person.SecondName;
                if (person.DateBirth.HasValue) this.DateBirth = person.DateBirth.Value;
                if (person.Image != null) this.PhotoSource = ImageSource.FromUri(person.Image);

                if (person is Models.DeadPerson) {
                    var deadPerson = (Models.DeadPerson)person;

                    this.PersonPosition = new Position(deadPerson.Latitude, deadPerson.Longitude);
                    if (deadPerson.DateDeath.HasValue) this.DateDeath = deadPerson.DateDeath.Value;
                    this.Height = deadPerson.Height;
                    this.Width = deadPerson.Width;
                    this.Text = deadPerson.Text;

                    if (deadPerson.Scheme != null) {
                        SchemeUrl = deadPerson.Scheme;
                    }



                    MapCenter = this.PersonPosition.Value;
                }
            }

            public ICommand ResetCommand => new Command(async () => {
                if (cancelTokenSource != null) return;

                StartLoading("Получение данных");

                HttpResponseMessage response;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Get, new Uri($"{Settings.RELATIVES_URL}{Id}/"), null, 30, cancelTokenSource.Token);
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

                var str = await response.Content.ReadAsStringAsync();

                try {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound) {
                        var msg = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Error>(str);
                        throw new HttpRequestException(msg.error);
                    }
                    response.EnsureSuccessStatusCode();
                } catch (UnauthorizedAccessException) {
                    await AuthHelper.ReloginAsync();
                    return;
                } catch (HttpRequestException ex) {
                    App.ToastNotificator.Show(ex.Message);
                    return;
                } catch {
                    App.ToastNotificator.Show("Ошибка");
                    return;
                }


                Models.Person person;
                try {
                    var json = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Person>(str);
                    person = Models.Person.Convert(json);
                } catch {
                    App.ToastNotificator.Show("Ошибка ответа сервера");
                    return;
                }

                SetData(person);
            });

            public ICommand SaveCommand => new Command(async () => {
                if (cancelTokenSource != null) return;

                if (this.IsDeadPerson && !this.PersonPosition.HasValue) {
                    App.ToastNotificator.Show("Не указано местоположение");
                    return;
                }
                if (string.IsNullOrWhiteSpace(this.FirstName)) {
                    App.ToastNotificator.Show("Поле \"Имя\" не заполнено");
                    return;
                }

                StartLoading("Сохранение");

                var content = new MultipartFormDataContent(String.Format("----------{0:N}", Guid.NewGuid()));
                content.Add(new StringContent(IsDeadPerson ? "dead" : "alive"), "type");
                content.Add(new StringContent(this.FirstName), "first_name");
                if (this.SecondName != null)
                    content.Add(new StringContent(this.SecondName), "second_name");
                if (this.LastName != null)
                    content.Add(new StringContent(this.LastName), "last_name");
                if (this.DateBirth.HasValue)
                    content.Add(new StringContent(this.DateBirth.Value.ToString("yyyy-MM-dd")), "date_birth");
                if (this.PhotoSource != null && this.PhotoSource is FileImageSource) {
                    var result = await DependencyService.Get<IFileStreamPicker>().GetResizedJpegAsync((PhotoSource as FileImageSource).File, 1000, 1000);
                    content.Add(new ByteArrayContent(result), "photo", "photo.jpg");
                }
                if (this.IsDeadPerson) {
                    content.Add(new StringContent(this.PersonPosition.Value.Latitude.ToString(CultureInfo.InvariantCulture)), "latitude");
                    content.Add(new StringContent(this.PersonPosition.Value.Longitude.ToString(CultureInfo.InvariantCulture)), "longitude");

                    if (this.DateDeath.HasValue)
                        content.Add(new StringContent(this.DateDeath.Value.ToString("yyyy-MM-dd")), "date_death");
                    if (this.Text != null)
                        content.Add(new StringContent(this.Text), "text");
                    if (this.Height.HasValue)
                        content.Add(new StringContent(this.Height.Value.ToString(CultureInfo.InvariantCulture)), "height");
                    if (this.Width.HasValue)
                        content.Add(new StringContent(this.Width.Value.ToString(CultureInfo.InvariantCulture)), "width");
                    if (this.SchemeStream != null) {
                        content.Add(new StreamContent(this.SchemeStream), "scheme", this.SchemeName);
                    }
                }



                HttpResponseMessage response;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Post, new Uri($"{Settings.RELATIVES_URL}{Id}/"), content, 60, cancelTokenSource.Token);
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

                var str = await response.Content.ReadAsStringAsync();

                try {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest
                        || response.StatusCode == System.Net.HttpStatusCode.NotFound
                        || response.StatusCode == System.Net.HttpStatusCode.Forbidden
                        || response.StatusCode == System.Net.HttpStatusCode.InternalServerError
                    ) {
                        var msg = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Error>(str);
                        throw new HttpRequestException(msg.error);
                    }
                    response.EnsureSuccessStatusCode();
                } catch (UnauthorizedAccessException) {
                    await AuthHelper.ReloginAsync();
                    return;
                } catch (HttpRequestException ex) {
                    App.ToastNotificator.Show(ex.Message);
                    return;
                } catch {
                    App.ToastNotificator.Show("Ошибка");
                    return;
                }


                Models.Person person;
                try {
                    var json = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Person>(str);
                    person = Models.Person.Convert(json);
                } catch {
                    App.ToastNotificator.Show("Ошибка ответа сервера");
                    return;
                }


                SetData(person);
                App.ToastNotificator.Show("Сохранено");
            });

            public ICommand DeleteCommand => new Command(async () => {
                if (cancelTokenSource != null) return;

                if (!await App.Current.MainPage.DisplayAlert("Удаление", "Вы уверены?", "Да", "Нет")) return;

                StartLoading("Удаление");

                HttpResponseMessage response;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Delete, new Uri($"{Settings.RELATIVES_URL}{Id}/"), null, 30, cancelTokenSource.Token);
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

                var str = await response.Content.ReadAsStringAsync();

                try {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound
                        || response.StatusCode == System.Net.HttpStatusCode.Forbidden
                        || response.StatusCode == System.Net.HttpStatusCode.InternalServerError
                    ) {
                        var msg = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Error>(str);
                        throw new HttpRequestException(msg.error);
                    }
                    response.EnsureSuccessStatusCode();
                } catch (UnauthorizedAccessException) {
                    await AuthHelper.ReloginAsync();
                    return;
                } catch (HttpRequestException ex) {
                    App.ToastNotificator.Show(ex.Message);
                    return;
                } catch {
                    App.ToastNotificator.Show("Ошибка");
                    return;
                }


                App.ToastNotificator.Show("Удалено");
                await App.GlobalPage.Pop();
            });
        }
    }
}