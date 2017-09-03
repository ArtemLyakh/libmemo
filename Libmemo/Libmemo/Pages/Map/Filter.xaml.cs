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

namespace Libmemo.Pages.Map
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Filter : ContentPage
    {
        private ViewModel Model {
            get => (ViewModel)BindingContext;
            set => BindingContext = value;
        }

        public Filter()
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

            public ViewModel() : base() {
                IsSearchButtonShow = true;



                EventHandler requireSearch = (sender, e) => {
                    IsSearchButtonShow = true;
                    IsShowButtonShow = false;

                    IsCityShow = false;
                    IsAddressShow = false;
                    IsSectionShow = false;
                    IsGraveNumberShow = false;
                };
                FioChanged += requireSearch;
                DateBirthChanged += requireSearch;
                DateDeathChanged += requireSearch;


                SearchCompleted += (sender, e) => {
                    App.ToastNotificator.Show($"Найдено элементов: {Data.Count}");
                    IsSearchButtonShow = false;
                    IsShowButtonShow = true;
                };


                SearchCompleted += (sender, e) => {
                    IsCityShow = true;
                    IsAddressShow = false;
                    IsSectionShow = false;
                    IsGraveNumberShow = false;

                    CityList = Data?.DefaultIfEmpty()
                        .Where(i => !string.IsNullOrWhiteSpace(i.City))
                        .Where(i => i != null)
                        .Select(i => i.City)
                        .Distinct()
                        .ToList();
                };
                CityChanged += (sender, e) => {
                    IsAddressShow = true;
                    IsSectionShow = false;
                    IsGraveNumberShow = false;

                    AddressList = Data?.DefaultIfEmpty()
                        .Where(i => i.City == City)
                        .Where(i => !string.IsNullOrWhiteSpace(i.Address))
                        .Where(i => i != null)
                        .Select(i => i.Address)
                        .Distinct()
                        .ToList();
                };
                AddressChanged += (sender, e) => {
                    IsSectionShow = true;
                    IsGraveNumberShow = false;

                    SectionList = Data?.DefaultIfEmpty()
                        .Where(i => i.City == City)
                        .Where(i => i.Address == Address)
                        .Where(i => !string.IsNullOrWhiteSpace(i.Section))
                        .Where(i => i != null)
                        .Select(i => i.Section)
                        .Distinct()
                        .ToList();
                };
                SectionChanged += (sender, e) => {
                    IsGraveNumberShow = true;

                    GraveNumberList = Data?.DefaultIfEmpty()
                        .Where(i => i.City == City)
                        .Where(i => i.Address == Address)
                        .Where(i => i.Section == Section)
                        .Where(i => i.GraveNumber.HasValue)
                        .Where(i => i != null)
                        .Select(i => i.GraveNumber.Value.ToString())
                        .Distinct()
                        .ToList();
                };


            }

            //                
            private event EventHandler FioChanged;
            private event EventHandler DateBirthChanged;
            private event EventHandler DateDeathChanged;
            private event EventHandler CityChanged;
            private event EventHandler AddressChanged;
            private event EventHandler SectionChanged;
            private event EventHandler GraveNumberChanged;

            private event EventHandler SearchCompleted;

            private string _fio;
            public string Fio {
                get => _fio;
                set {
                    if (_fio != value) {
                        _fio = value;
                        OnPropertyChanged(nameof(Fio));
                        FioChanged?.Invoke(this, null);
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
                        DateBirthChanged?.Invoke(this, null);
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
                        DateDeathChanged?.Invoke(this, null);
                    }
                }
            }

            private List<string> _cityList;
            public List<string> CityList {
                get => _cityList;
                set {
                    if (_cityList != value) {
                        _cityList = value;
                        OnPropertyChanged(nameof(CityList));
                    }
                }
            }
            private int _cityListIndex = -1;
            public int CityListIndex {
                get => _cityListIndex;
                set {
                    _cityListIndex = value;
                    OnPropertyChanged(nameof(CityListIndex));
                    if (value != -1) CityChanged?.Invoke(this, null);
                }
            }
            public string City => CityListIndex == -1 ? null : CityList.ElementAtOrDefault(CityListIndex);

            private List<string> _addressList;
            public List<string> AddressList {
                get => _addressList;
                set {
                    if (_addressList != value) {
                        _addressList = value;
                        OnPropertyChanged(nameof(AddressList));
                    }
                }
            }
            private int _addressListIndex = -1;
            public int AddressListIndex {
                get => _addressListIndex;
                set {
                    _addressListIndex = value;
                    OnPropertyChanged(nameof(AddressListIndex));
                    if (value != -1) AddressChanged?.Invoke(this, null);
                }
            }
            public string Address => AddressListIndex == -1 ? null : AddressList.ElementAtOrDefault(AddressListIndex);

            private List<string> _sectionList;
            public List<string> SectionList {
                get => _sectionList;
                set {
                    if (_sectionList != value) {
                        _sectionList = value;
                        OnPropertyChanged(nameof(SectionList));
                    }
                }
            }
            private int _sectionListIndex = -1;
            public int SectionListIndex {
                get => _sectionListIndex;
                set {
                    _sectionListIndex = value;
                    OnPropertyChanged(nameof(SectionListIndex));
                    if (value != -1) SectionChanged?.Invoke(this, null);
                }
            }
            public string Section => SectionListIndex == -1 ? null : SectionList.ElementAtOrDefault(SectionListIndex);

            private List<string> _graveNumberList;
            public List<string> GraveNumberList {
                get => _graveNumberList;
                set {
                    if (_graveNumberList != value) {
                        _graveNumberList = value;
                        OnPropertyChanged(nameof(GraveNumberList));
                    }
                }
            }
            private int _graveNumberListIndex = -1;
            public int GraveNumberListIndex {
                get => _graveNumberListIndex;
                set {
                    _graveNumberListIndex = value;
                    OnPropertyChanged(nameof(GraveNumberListIndex));
                    if (value != -1) GraveNumberChanged?.Invoke(this, null);
                }
            }
            public string GraveNumber => GraveNumberListIndex == -1 ? null : GraveNumberList.ElementAtOrDefault(GraveNumberListIndex);

            private bool _isSeachButtonShow;
            public bool IsSearchButtonShow {
                get => _isSeachButtonShow;
                set {
                    if (_isSeachButtonShow != value) {
                        _isSeachButtonShow = value;
                        OnPropertyChanged(nameof(IsSearchButtonShow));
                    }
                }
            }

            private bool _isShowButtonShow;
            public bool IsShowButtonShow {
                get => _isShowButtonShow;
                set {
                    if (_isShowButtonShow != value) {
                        _isShowButtonShow = value;
                        OnPropertyChanged(nameof(IsShowButtonShow));
                    }
                }
            }

            private bool _isCityShow;
            public bool IsCityShow {
                get => _isCityShow;
                set {
                    if (_isCityShow != value) {
                        _isCityShow = value;
                        OnPropertyChanged(nameof(IsCityShow));
                    }
                }
            }

            private bool _isAddressShow;
            public bool IsAddressShow {
                get => _isAddressShow;
                set {
                    if (_isAddressShow != value) {
                        _isAddressShow = value;
                        OnPropertyChanged(nameof(IsAddressShow));
                    }
                }
            }

            private bool _isSectionShow;
            public bool IsSectionShow {
                get => _isSectionShow;
                set {
                    if (_isSectionShow != value) {
                        _isSectionShow = value;
                        OnPropertyChanged(nameof(IsSectionShow));
                    }
                }
            }

            private bool _isGraveNumberShow;
            public bool IsGraveNumberShow {
                get => _isGraveNumberShow;
                set {
                    if (_isGraveNumberShow != value) {
                        _isGraveNumberShow = value;
                        OnPropertyChanged(nameof(IsGraveNumberShow));
                    }
                }
            }



            private List<Models.DeadPerson> Data;
            public ICommand SearchCommand => new Command(async () => {
                if (cancelTokenSource != null) return;

                if (string.IsNullOrWhiteSpace(Fio)) {
                    App.ToastNotificator.Show("Поле \"ФИО\" не заполнено");
                    return;
                }


                var paramList = new Dictionary<string, string> { { "fio", Fio } };
                if (DateBirth.HasValue) {
                    paramList.Add("date_birth", DateBirth.Value.ToString("yyyy-MM-dd"));
                }
                if (DateDeath.HasValue) {
                    paramList.Add("date_death", DateDeath.Value.ToString("yyyy-MM-dd"));
                }
                var uri = new UriBuilder(Settings.MAP_URL) {
                    Query = string.Join("&", paramList.Select(i => i.Key + "=" + i.Value))
                }.Uri;

                StartLoading("Поиск");

                HttpResponseMessage response;
                try {
                    cancelTokenSource = new CancellationTokenSource();
                    response = await WebClient.Instance.SendAsync(HttpMethod.Get, uri, null, 30, cancelTokenSource.Token);
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
                    response.EnsureSuccessStatusCode();
                } catch {
                    App.ToastNotificator.Show("Ошибка");
                    return;
                }

                List<Models.DeadPerson> personList;
                try {
                    var json = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Json.Person>>(str);
                    personList = json.Select(i => Models.DeadPerson.ConvertFromJson(i)).ToList();
                } catch {
                    App.ToastNotificator.Show("Ошибка ответа сервера");
                    return;
                }


                Data = personList;
                SearchCompleted?.Invoke(this, null);
            });

            public ICommand ShowCommand => new Command(async () => {
                if (Data == null) {
                    App.ToastNotificator.Show("Отсутствуют элементы");
                    return;
                }

                var elements = Data.AsEnumerable();
                if (City != null) elements = elements.Where(i => i.City == City);
                if (Address != null) elements = elements.Where(i => i.Address == Address);
                if (Section != null) elements = elements.Where(i => i.Section == Section);
                if (GraveNumber != null) elements = elements.Where(i => i.GraveNumber.HasValue && i.GraveNumber.Value.ToString() == GraveNumber);

                var list = elements.ToList();

                await App.GlobalPage.Push(new Pages.Map.Map(list));
            });





        }
    }
}