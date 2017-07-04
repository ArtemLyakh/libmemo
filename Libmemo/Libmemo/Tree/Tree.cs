using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Libmemo {
    class Tree {
        private readonly int _userId;
        public int UserId => _userId;

        public class Item {
            public Item(Person person) => this.Person = person;
            public Person Person { get; set; }
            public Item Mother { get; set; }
            public Item Father { get; set; }
            public List<Person> Siblings { get; set; } = new List<Person>();
        }
        public Item Root { get; set; } = null;


        public Tree (int userId) {
            this._userId = userId;
        }


        public class Json {
            public int? user { get; set; }
            public List<Row> data { get; set; }

            public class Row {
                public int? person { get; set; }
                public int? mother { get; set; }
                public int? father { get; set; }
                public List<int?> siblings { get; set; }
            }
        }
        public async Task<bool> LoadData() {
            try {
                using (var handler = new HttpClientHandler { CookieContainer = AuthHelper.CookieContainer })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) })
                using (var responce = await client.GetAsync(Settings.TREE_DATA_URL)) {
                    var str = await responce.Content.ReadAsStringAsync();
                    if (responce.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }

                    var json = Newtonsoft.Json.JsonConvert.DeserializeObject<Json>(str);
                    await DecodeJson(json);

                    responce.EnsureSuccessStatusCode();
                    return true;
                }
            } catch (UnauthorizedAccessException) {
                throw;
            } catch (FormatException) {
                throw;
            } catch {
                return false;
            }
        }
        private async Task DecodeJson(Json data) {
            var personDict = await App.Database.GetDictionary();

            if (!data.user.HasValue || !personDict.ContainsKey(data.user.Value)) 
                throw new FormatException();

            var jsonDict = new Dictionary<int, Json.Row>();
            foreach (var item in data.data) {
                if (!item.person.HasValue) continue;
                jsonDict[item.person.Value] = item;
            }

            var decoded = new HashSet<int>();
            Item DecodeItem(int id) {
                if (!jsonDict.ContainsKey(id)
                    || !personDict.ContainsKey(id)
                    || decoded.Contains(id)
                    ) return null;

                decoded.Add(id);

                return new Item(personDict[id]) {
                    Mother = jsonDict[id].mother.HasValue ? DecodeItem(jsonDict[id].mother.Value) : null,
                    Father = jsonDict[id].father.HasValue ? DecodeItem(jsonDict[id].father.Value) : null,
                    Siblings = jsonDict[id].siblings
                        .Where(i => i.HasValue && personDict.ContainsKey(i.Value))
                        .Select(i => personDict[i.Value])
                        .ToList()
                };
            }
          

            Root = DecodeItem(data.user.Value);
        }



        private const int LINE_WIDTH = 5;

        private const int ADD_BUTTON_WIDTH = 50;
        private const int ADD_BUTON_HEIGHT = 50;

        private const int TREE_ITEM_WIDTH = 100;
        private const int TREE_ITEM_HEIGHT = 175;
        private const int TREE_ITEM_IMAGE_HEIGHT = 100;

        private const int SPACE_BETWEEN_ITEMS = 15;
        private const int SPACE_BETWEEN_GROUPS = 50;
        private const int LEVEL_HEIGHT = 250;



        public void DrawTree(AbsoluteLayout layout) {
            var LineList = new List<(View, Point)>();
            var ViewList = new List<(View, Point)>();

            var levelOffsetDict = new Dictionary<int, double>();
            (Point, double) DrawItem(Item item, int level, bool left) {
                if (!levelOffsetDict.ContainsKey(level)) levelOffsetDict[level] = 0;

                if (item == null) {
                    var buttonData = GetAddNewButton(new Point(levelOffsetDict[level], level * LEVEL_HEIGHT), () => {
                        var q = 1;
                    });
                    ViewList.Add(buttonData);
                    levelOffsetDict[level] += ADD_BUTTON_WIDTH / 2;
                    var end = levelOffsetDict[level];


                    levelOffsetDict[level] += SPACE_BETWEEN_GROUPS;
                    return (buttonData.Item2, end);
                } else {

                }

                //var offset = DrawItem(item.Mother, level + 1, true);

                //if (item.Mother != null)

                //var person = item.Person;
                //var point = new Point(TREE_ITEM_WIDTH/2 + levelDict[level] * TREE_ITEM_WIDTH, 1000 - level * TREE_ITEM_HEIGHT);
                //Action action = () => {

                //    var q = 1;
                //};

                //(var v, var p) = GetTreeItem(point, person, action);
                //layout.Children.Add(v, p);

                //if (item.Mother != null) DrawItem(item.Mother, level + 1);
                //if (item.Father != null) DrawItem(item.Father, level + 1);
            }




            layout.Children.Clear();
            layout.WidthRequest = 1500;
            layout.HeightRequest = 1500;

            if (Root != null) DrawItem(Root, 0);

        }


        private (View, Point) GetLine(Point A, Point B) {
            var length = Math.Pow(Math.Pow(A.Y - B.Y, 2) + Math.Pow(A.X - B.X, 2), 0.5);
            var rot = Math.Atan((B.X - A.X) / (A.Y - B.Y)) * 180 / Math.PI;

            var view = new BoxView {
                HeightRequest = length,
                WidthRequest = LINE_WIDTH,
                BackgroundColor = Color.Black,
                Rotation = rot
            };
            var point = new Point((A.X + B.X) / 2 - LINE_WIDTH / 2, (A.Y + B.Y) / 2 - length / 2);

            return (view, point);
        }

        private (View, Point) GetAddNewButton(Point center, Action onTap) {
            var button = new Image {
                WidthRequest = ADD_BUTTON_WIDTH,
                HeightRequest = ADD_BUTON_HEIGHT,
                Source = ImageSource.FromResource("Libmemo.Tree.Images.add_button.jpg"),
            };
            button.GestureRecognizers.Add(new TapGestureRecognizer {
                Command = new Command(() => onTap?.Invoke())
            });

            var point = new Point(center.X - ADD_BUTTON_WIDTH / 2, center.Y - ADD_BUTON_HEIGHT / 2);

            return (button, point);
        }

        private (View, Point) GetTreeItem (Point center, Person person, Action onTap) {
            var stack = new StackLayout {
                HeightRequest = TREE_ITEM_HEIGHT,
                WidthRequest = TREE_ITEM_WIDTH
            };
            stack.GestureRecognizers.Add(new TapGestureRecognizer {
                Command = new Command(() => onTap?.Invoke())
            });

            var image = new Image {
                HeightRequest = TREE_ITEM_IMAGE_HEIGHT,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Source = person.IconUrl != null 
                    ? ImageSource.FromUri(person.IconUrl) 
                    : ImageSource.FromResource("Libmemo.Tree.Images.no_photo.jpg")
            };

            stack.Children.Add(image);

            var fio = new Label {
                Text = person.FIO,
                HeightRequest = TREE_ITEM_HEIGHT - TREE_ITEM_IMAGE_HEIGHT,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                LineBreakMode = LineBreakMode.WordWrap
            };
            stack.Children.Add(fio);

            var point = new Point(center.X - TREE_ITEM_WIDTH / 2, center.Y - TREE_ITEM_HEIGHT / 2);

            return (stack, point);
        }






    }

}
