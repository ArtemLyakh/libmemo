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

            public int[] Columns { get; set; }
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
        private const int SPACE_BETWEEN_GROUPS = 150;
        private const int LEVEL_HEIGHT = 250;

        private int ColumnCount { get; set; }
        private Dictionary<int, double> ColumnWidths { get; set; } = new Dictionary<int, double>();
        private int LevelCount { get; set; }
        private double LayoutWidth { get; set; }
        private double LayoutHeight { get; set; }
        private List<(View, Point)> Lines { get; set; } = new List<(View, Point)>();
        private List<(View, Point)> Views { get; set; } = new List<(View, Point)>();

        public void DrawTreeTest(AbsoluteLayout layout) {
            CalculateColumns();
            SetDefaultColumnWidths();
            AdjustColumnWidths();
            CalculateLevels();
            CalculateLayoutSize();
            CalculatePositions();
            DrawLayout(layout);
        }
        private void CalculateColumns() {
            ColumnCount = 0;

            int[] Iteration(Item item) => item == null
                ? new int[] { ++ColumnCount }
                : item.Columns = Iteration(item.Mother).Concat(Iteration(item.Father)).ToArray();

            Iteration(Root);
        }
        private void SetDefaultColumnWidths() {
            for (int i = 1; i <= ColumnCount; i++) {
                ColumnWidths[i] = SPACE_BETWEEN_GROUPS / 2 + ADD_BUTTON_WIDTH + SPACE_BETWEEN_GROUPS / 2;
            }
        }
        private void AdjustColumnWidths() {
            void Iteration(Item item) {
                if (item != null) {
                    double requiredWidth = SPACE_BETWEEN_GROUPS + TREE_ITEM_WIDTH + SPACE_BETWEEN_ITEMS + ADD_BUTTON_WIDTH;
                    foreach (var sibling in item.Siblings) 
                        requiredWidth += SPACE_BETWEEN_ITEMS + TREE_ITEM_WIDTH;

                    var currentColumnWidth = ColumnWidths.Keys.Intersect(item.Columns).Aggregate(0d, (sum, i) => sum += ColumnWidths[i]);
                    var widthDificit = requiredWidth - currentColumnWidth;
                    if (widthDificit > 0) {
                        var additionalColumnWidth = widthDificit / item.Columns.Length;
                        foreach (var i in item.Columns) {
                            ColumnWidths[i] += additionalColumnWidth;
                        }
                    }

                    Iteration(item.Mother);
                    Iteration(item.Father);
                }
            }

            Iteration(Root);
        }
        private void CalculateLevels() {
            LevelCount = 0;
            void Iteration(Item item, int level)
            {
                if (item == null) {
                    LevelCount = Math.Max(LevelCount, level);
                } else {
                    Iteration(item.Mother, level + 1);
                    Iteration(item.Father, level + 1);
                }
            }

            Iteration(Root, 1);
        }
        private void CalculateLayoutSize() {
            LayoutHeight = LEVEL_HEIGHT * LevelCount;
            LayoutWidth = ColumnWidths.Select(i => i.Value).Aggregate(0d, (sum, i) => sum += i);
        }
        private void CalculatePositions() {
            Views = new List<(View, Point)>();
            Lines = new List<(View, Point)>();

            Point IterationAdd(int column, int level, Item child, Action action) {
                double offset = 0;
                for (int i = 1; i < column; i++)
                    offset += ColumnWidths[i];
                var width = ColumnWidths[column];

                var x = offset + width / 2;
                var y = LayoutHeight - LEVEL_HEIGHT / 2 - (level - 1) * LEVEL_HEIGHT;

                var bottomConnectPoint = new Point(x, y + ADD_BUTON_HEIGHT / 2);
                (View, Point) element = GetAddNewButton(new Point(x, y), () => {
                    var q = 1;
                });
                Views.Add(element);
                return bottomConnectPoint;
            }

            Point Iteration(Item item, int level) {
                double offset = 0, groupWidth = 0;
                int columnStart = item.Columns.Min();
                int columnEnd = item.Columns.Max();
                for (int i = 1; i <= columnEnd; i++)
                    if (i < columnStart) offset += ColumnWidths[i];
                    else groupWidth += ColumnWidths[i];

                double groupRequiredWidth = SPACE_BETWEEN_GROUPS + TREE_ITEM_WIDTH + SPACE_BETWEEN_ITEMS + ADD_BUTTON_WIDTH;
                foreach (var sibling in item.Siblings)
                    groupRequiredWidth += SPACE_BETWEEN_ITEMS + TREE_ITEM_WIDTH;

                //pointer
                var x = offset + SPACE_BETWEEN_GROUPS / 2;
                x += groupWidth / 2;
                x -= groupRequiredWidth / 2;
                var y = LayoutHeight - LEVEL_HEIGHT / 2 - (level - 1) * LEVEL_HEIGHT;

                Point MotherConnectPoint, FatherConnectPoint;
                if (item.Mother == null) {
                    MotherConnectPoint = IterationAdd(item.Columns.First(), level + 1, item, () => {
                        var q = 1;
                    });
                } else {
                    MotherConnectPoint = Iteration(item.Mother, level + 1);
                }
                if (item.Father == null) {
                    FatherConnectPoint = IterationAdd(item.Columns.Last(), level + 1, item, () => {
                        var q = 1;
                    });
                } else {
                    FatherConnectPoint = Iteration(item.Father, level + 1);
                }

                #region Draw
                (View, Point) element;

                #region Current item
                x += TREE_ITEM_WIDTH / 2;
                var bottomConnectPoint = new Point(x, y + TREE_ITEM_HEIGHT / 2);
                var topConnectPoint = new Point(x, y - TREE_ITEM_HEIGHT / 2);
                element = GetTreeItem(new Point(x, y), item.Person, () => {
                    var q = 1;
                });
                Views.Add(element);
                x += TREE_ITEM_WIDTH / 2;
                #endregion

                #region Connect lines
                element = GetLine(MotherConnectPoint, topConnectPoint);
                Lines.Add(element);
                element = GetLine(FatherConnectPoint, topConnectPoint);
                Lines.Add(element);
                #endregion

                #region Siblings
                foreach (var sibling in item.Siblings) {
                    #region Line
                    element = GetLine(new Point(x, y), new Point(x + SPACE_BETWEEN_ITEMS, y));
                    Lines.Add(element);
                    x += SPACE_BETWEEN_ITEMS;
                    #endregion

                    #region Sibling
                    x += TREE_ITEM_WIDTH / 2;
                    element = GetTreeItem(new Point(x, y), sibling, () => {
                        var q = 1;
                    });
                    Views.Add(element);
                    x += TREE_ITEM_WIDTH / 2;
                    #endregion
                }
                #endregion

                #region Add button

                #region Line
                element = GetLine(new Point(x, y), new Point(x + SPACE_BETWEEN_ITEMS, y));
                Lines.Add(element);
                x += SPACE_BETWEEN_ITEMS;
                #endregion

                #region Button
                x += ADD_BUTTON_WIDTH;
                element = GetAddNewButton(new Point(x, y), () => {
                    var q = 1;
                });
                Views.Add(element);
                #endregion

                #endregion

                #endregion

                return bottomConnectPoint;
            }

            Iteration(Root, 1);
        }
        private void DrawLayout(AbsoluteLayout layout) {
            layout.Children.Clear();

            layout.HeightRequest = LayoutHeight;
            layout.WidthRequest = LayoutWidth;

            foreach (var element in Lines) 
                layout.Children.Add(element.Item1, element.Item2);

            foreach (var element in Views)
                layout.Children.Add(element.Item1, element.Item2);
        }









        public void DrawTree(AbsoluteLayout layout) {


            var levelDict = new Dictionary<int, int>();
            void DrawItem(Item item, int level)
            {
                if (!levelDict.ContainsKey(level)) levelDict[level] = 0;
                else levelDict[level] += 1;

                var person = item.Person;
                var point = new Point(TREE_ITEM_WIDTH / 2 + levelDict[level] * TREE_ITEM_WIDTH, 1000 - level * TREE_ITEM_HEIGHT);
                Action action = () => {

                    var q = 1;
                };

                (var v, var p) = GetTreeItem(point, person, action);
                layout.Children.Add(v, p);

                if (item.Mother != null) DrawItem(item.Mother, level + 1);
                if (item.Father != null) DrawItem(item.Father, level + 1);
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
                WidthRequest = TREE_ITEM_WIDTH,
                BackgroundColor = Color.Red
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
