﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFImageLoading.Forms;
using FFImageLoading.Transformations;
using Xamarin.Forms;

namespace Libmemo.Helpers
{
    public class Tree
    {
        private AbsoluteLayout Layout { get; set; }
        private ScrollView Scroll { get; set; }
        public Item.NormalItem Root { get; set; }

        public abstract class Item
        {
            public int Id { get; set; }
            public string Fio { get; set; }
            public ImageSource Image;
            public bool IsAlive { get; set; }

            public Item(int id, string fio, bool isAlive, string image_uri = null)
            {
                Id = id;
                Fio = fio;
                IsAlive = isAlive;
                Image = image_uri != null && Uri.TryCreate(image_uri, UriKind.Absolute, out Uri uri)
                    ? ImageSource.FromUri(uri)
                    : ImageSource.FromFile("no_tree_img");
			}


            public class SiblingItem : Item
            {
                public SiblingItem(int id, string fio, bool isAlive, string image_uri = null) : base(id, fio, isAlive, image_uri) { }
            }


            public class NormalItem : Item
            {
                public NormalItem(int id, string fio, bool isAlive, string image_uri = null) : base(id, fio, isAlive, image_uri) { }

				public NormalItem Mother { get; set; }
				public NormalItem Father { get; set; }
				public List<SiblingItem> Siblings { get; set; } = new List<SiblingItem>();

				public int[] Columns { get; set; }
            }

        }


        public Tree(AbsoluteLayout absolute, ScrollView scroll)
        {
            this.Layout = absolute;
            this.Scroll = scroll;
        }

        private string ConstructFio(string firstName, string secondName, string lastname)
        {
            var fio = new List<string>();
            if (!string.IsNullOrWhiteSpace(lastname)) fio.Add(lastname);
            fio.Add(firstName);
            if (!string.IsNullOrWhiteSpace(secondName)) fio.Add(secondName);

            return string.Join(" ", fio);
        }
        private string ConstructFioFromPerson(Json.UserListEntry person)
        {
            return ConstructFio(person.first_name, person.last_name, person.second_name);
        }

        private void LoadFromJson(Json.Tree data)
        {

            Item.NormalItem DecodeItem(int id)
            {
                var structure = data.structure[id];
                var person = data.persons[id];

                var item = new Item.NormalItem(person.id, ConstructFioFromPerson(person), person.type == "alive", person.preview_image_url);

                if (structure.mother.HasValue) item.Mother = DecodeItem(structure.mother.Value);
                if (structure.father.HasValue) item.Father = DecodeItem(structure.father.Value);

                foreach (var sibling in structure.siblings)
                {
                    var siblingPerson = data.persons[sibling];
                    item.Siblings.Add(new Item.SiblingItem(siblingPerson.id, ConstructFioFromPerson(siblingPerson), siblingPerson.type == "alive", siblingPerson.preview_image_url));
                }

                return item;
            }

            Root = DecodeItem(data.user);
        }


        public async Task DrawTree(Json.Tree data)
        {
            LoadFromJson(data);
            await _DrawTree();
        }

        private async Task RedrawTree()
        {
            await _DrawTree();
        }

        #region Drawing
        private async Task _DrawTree() 
        {
			await Task.Run(() => {
				CalculateColumns();
				SetDefaultColumnWidths();
				AdjustColumnWidths();
				CalculateLevels();
				CalculateLayoutSize();
				BufferDraw(true);				
			});
            DrawLayout();
			await PositionToCenter();
        }

		private const int LINE_WIDTH = 4;
		private const int BORDER_THICKNESS = 10;

		private const int ADD_BUTTON_WIDTH = 50;
		private const int ADD_BUTON_HEIGHT = 50;

		private const int TREE_ITEM_WIDTH = 75;
		private const int TREE_ITEM_HEIGHT = 75;

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

		private double _zoom = 1;
		private const double ZOOM_STEP = 0.75;
		public async Task SetZoom(double zoom, bool animate = false)
		{
			if (zoom <= 0.01) return;
			if (zoom > 1) zoom = 1;
			_zoom = zoom;

			if (Layout != null)
			{
				if (animate) await Layout.ScaleTo(zoom);
				else Layout.Scale = zoom;
			}
		}
		public async Task ZoomIn() => await SetZoom(_zoom / ZOOM_STEP, true);
		public async Task ZoomOut() => await SetZoom(_zoom * ZOOM_STEP, true);

		private void CalculateColumns()
		{
			ColumnCount = 0;

            int[] Iteration(Item.NormalItem item) => item == null
				? new int[] { ++ColumnCount }
				: item.Columns = Iteration(item.Mother).Concat(Iteration(item.Father)).ToArray();

			Iteration(Root);
		}
		private void SetDefaultColumnWidths()
		{
			for (int i = 1; i <= ColumnCount; i++) {
				ColumnWidths[i] = SPACE_BETWEEN_GROUPS / 2 + ADD_BUTTON_WIDTH + SPACE_BETWEEN_GROUPS / 2;
			}
		}
		private void AdjustColumnWidths()
		{
            void Iteration(Item.NormalItem item)
			{
				if (item != null)
				{
					double requiredWidth = SPACE_BETWEEN_GROUPS + TREE_ITEM_WIDTH + SPACE_BETWEEN_ITEMS + ADD_BUTTON_WIDTH;
					foreach (var sibling in item.Siblings)
						requiredWidth += SPACE_BETWEEN_ITEMS + TREE_ITEM_WIDTH;

					var currentColumnWidth = ColumnWidths.Keys.Intersect(item.Columns).Aggregate(0d, (sum, i) => sum += ColumnWidths[i]);
					var widthDificit = requiredWidth - currentColumnWidth;
					if (widthDificit > 0)
					{
						var additionalColumnWidth = widthDificit / item.Columns.Length;
						foreach (var i in item.Columns)
						{
							ColumnWidths[i] += additionalColumnWidth;
						}
					}

					Iteration(item.Mother);
					Iteration(item.Father);
				}
			}

			Iteration(Root);
		}
		private void CalculateLevels()
		{
			LevelCount = 0;
            void Iteration(Item.NormalItem item, int level)
			{
				if (item == null)
				{
					LevelCount = Math.Max(LevelCount, level);
				}
				else
				{
					Iteration(item.Mother, level + 1);
					Iteration(item.Father, level + 1);
				}
			}

			Iteration(Root, 1);
		}
		private void CalculateLayoutSize()
		{
			LayoutHeight = LEVEL_HEIGHT * LevelCount;
			LayoutWidth = ColumnWidths.Select(i => i.Value).Aggregate(0d, (sum, i) => sum += i);
		}
		private void BufferDraw(bool editable)
		{
			Views = new List<(View, Point)>();
			Lines = new List<(View, Point)>();

            Point IterationAddButton(int column, int level, Item.NormalItem child, Action action)
			{
				double offset = 0;
				for (int i = 1; i < column; i++)
					offset += ColumnWidths[i];
				var width = ColumnWidths[column];

				var x = offset + width / 2;
				var y = LayoutHeight - LEVEL_HEIGHT / 2 - (level - 1) * LEVEL_HEIGHT;

				var bottomConnectPoint = new Point(x, y + ADD_BUTON_HEIGHT / 2);

				(View, Point) element = GetAddNewButton(new Point(x, y), action);
				Views.Add(element);
				return bottomConnectPoint;
			}

            Point Iteration(Item.NormalItem item, int level)
			{
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

				Point MotherConnectPoint = item.Mother == null
					? IterationAddButton(item.Columns.First(), level + 1, item, null) //() => AddClickHandler(item.Person, AddPersonType.Mother)
					: Iteration(item.Mother, level + 1);

				Point FatherConnectPoint = item.Father == null
					? IterationAddButton(item.Columns.Last(), level + 1, item, null) //() => AddClickHandler(item.Person, AddPersonType.Father)
					: Iteration(item.Father, level + 1);

				#region Draw
				(View, Point) element;

				#region Current item
				x += TREE_ITEM_WIDTH / 2;
				var bottomConnectPoint = new Point(x, y + TREE_ITEM_HEIGHT / 2);
				var topConnectPoint = new Point(x, y - TREE_ITEM_HEIGHT / 2);
				var levelLine = y - LEVEL_HEIGHT / 2;
				element = GetTreeItem(new Point(x, y), item, null); //() => TreeItemClickHandler(item.Person)
				Views.Add(element);
				x += TREE_ITEM_WIDTH / 2;
				#endregion

				#region Connect lines
				Lines.Add(GetLine(topConnectPoint, new Point(topConnectPoint.X, levelLine)));
				Lines.Add(GetLine(new Point(MotherConnectPoint.X, levelLine + LINE_WIDTH / 2), MotherConnectPoint));
				Lines.Add(GetLine(new Point(FatherConnectPoint.X, levelLine + LINE_WIDTH / 2), FatherConnectPoint));
				Lines.Add(GetLine(
					new Point(Math.Min(MotherConnectPoint.X - LINE_WIDTH / 2, topConnectPoint.X), levelLine),
					new Point(Math.Max(FatherConnectPoint.X + LINE_WIDTH / 2, topConnectPoint.X), levelLine)
				));
				#endregion

				#region Siblings
				foreach (var sibling in item.Siblings)
				{
					#region Line
					Lines.Add(GetLine(new Point(x, y), new Point(x + SPACE_BETWEEN_ITEMS, y)));
					x += SPACE_BETWEEN_ITEMS;
					#endregion

					#region Sibling
					x += TREE_ITEM_WIDTH / 2;
					element = GetTreeItem(new Point(x, y), sibling, null); //() => TreeItemClickHandler(sibling)
					Views.Add(element);
					x += TREE_ITEM_WIDTH / 2;
					#endregion
				}
				#endregion

				#region Add button

				#region Line
				Lines.Add(GetLine(new Point(x, y), new Point(x + SPACE_BETWEEN_ITEMS + ADD_BUTTON_WIDTH / 2, y)));
				x += SPACE_BETWEEN_ITEMS;
				#endregion

				#region Button
				x += ADD_BUTTON_WIDTH / 2;
				element = GetAddNewButton(new Point(x, y), null); //() => AddClickHandler(item.Person, AddPersonType.Sibling)
				Views.Add(element);
				#endregion

				#endregion

				#endregion

				return bottomConnectPoint;
			}

			Iteration(Root, 1);
		}
		private void DrawLayout()
		{
			Layout.Children.Clear();

			Layout.HeightRequest = LayoutHeight;
			Layout.WidthRequest = LayoutWidth;

			foreach (var element in Lines)
				Layout.Children.Add(element.Item1, element.Item2);

			foreach (var element in Views)
				Layout.Children.Add(element.Item1, element.Item2);
		}
		private async Task PositionToCenter()
		{
			var w = Scroll.Width / LayoutWidth;
			var h = Scroll.Height / LayoutHeight;

			await Scroll.ScrollToAsync(LayoutWidth / 2 - Scroll.Width / 2, LayoutHeight / 2 - Scroll.Height / 2, false);
			await SetZoom(Math.Min(w, h));
		}




		private enum AddPersonType
		{
			Mother, Father, Sibling
		}
		private async void AddClickHandler(Item person, AddPersonType type)
		{
			//var page = new SelectPersonExceptPage(presentPersonIds);
			//page.ItemSelected += async (sender, selected) => {
			//	await App.GlobalPage.Pop();

			//	var item = SearchInTree(person.Id);
			//	if (item == null) return;
			//	switch (type)
			//	{
			//		case AddPersonType.Sibling:
			//			item.Siblings.Add(selected);
			//			break;
			//		case AddPersonType.Mother:
			//			item.Mother = new Item(selected);
			//			break;
			//		case AddPersonType.Father:
			//			item.Father = new Item(selected);
			//			break;
			//	}
			//	RedrawTree();
			//};

			//await App.GlobalPage.Push(page);
		}

		private enum TreeItemAction
		{
			Cancel,
			Details, Replace, Delete
		}

		private async void TreeItemClickHandler(Person person)
		{
			var actions = new Dictionary<string, TreeItemAction> {
				{ "Просмотреть", TreeItemAction.Details },
				{ "Заменить", TreeItemAction.Replace },
				{ "Удалить", TreeItemAction.Delete }
			};
			var cancel = new KeyValuePair<string, TreeItemAction>("Отмена", TreeItemAction.Cancel);

			var action = (await App.Current.MainPage.DisplayActionSheet(
				person.FIO,
				cancel.Key,
				null,
				actions.Select(i => i.Key).ToArray())
			) ?? cancel.Key;

			actions.Add(cancel.Key, cancel.Value);

			var selectedAction = actions.ContainsKey(action) ? actions[action] : cancel.Value;
			switch (selectedAction)
			{
				case TreeItemAction.Details:
					TreeItemDetails(person);
					break;
				case TreeItemAction.Replace:
					TreeItemReplace(person);
					break;
				case TreeItemAction.Delete:
					TreeItemDelete(person);
					break;
				case TreeItemAction.Cancel:
				default:
					return;
			}
		}
		private async void TreeItemDetails(Person person)
		{
            throw new NotImplementedException();
			//var page = new DetailPage(person.Id);
			//await App.GlobalPage.Push(page);
		}
		private async void TreeItemReplace(Person person)
		{
            throw new NotImplementedException();
			//if (person.Id == Root.Person.Id)
			//{
			//	App.ToastNotificator.Show("Невозможно заменить корневой элемент");
			//	return;
			//}

			//var presentPersonIds = this.GetInTreePersonIds();
			//var page = new SelectPersonExceptPage(presentPersonIds);
			//page.ItemSelected += async (sender, selected) => {
			//	await App.GlobalPage.Pop();

			//	var item = SearchInTree(person.Id);
			//	item.Person = selected;
			//	RedrawTree();
			//};

			//await App.GlobalPage.Push(page);
		}
		private void TreeItemDelete(Person person)
		{
            throw new NotImplementedException();
			//if (person.Id == Root.Person.Id)
			//{
			//	App.ToastNotificator.Show("Невозможно удалить корневой элемент");
			//	return;
			//}

			//void Iteration(Item item)
			//{
			//	if (item.Mother != null && item.Mother.Person.Id == person.Id)
			//	{
			//		item.Mother = null;
			//	}
			//	if (item.Father != null && item.Father.Person.Id == person.Id)
			//	{
			//		item.Father = null;
			//	}
			//	item.Siblings.RemoveAll(i => i.Id == person.Id);

			//	if (item.Mother != null) Iteration(item.Mother);
			//	if (item.Father != null) Iteration(item.Father);
			//}
			//Iteration(Root);

			//RedrawTree();
		}

		private (View, Point) GetLine(Point A, Point B)
		{
			var length = Math.Pow(Math.Pow(A.Y - B.Y, 2) + Math.Pow(A.X - B.X, 2), 0.5);
			var rot = Math.Atan((B.X - A.X) / (A.Y - B.Y)) * 180 / Math.PI;

			var view = new BoxView
			{
				HeightRequest = length,
				WidthRequest = LINE_WIDTH,
				BackgroundColor = Color.FromHex("D4D4D4"),
				Rotation = rot
			};
			var point = new Point((A.X + B.X) / 2 - LINE_WIDTH / 2, (A.Y + B.Y) / 2 - length / 2);

			return (view, point);
		}
		private (View, Point) GetAddNewButton(Point center, Action onTap = null)
		{
			var button = new CachedImage
			{
				WidthRequest = ADD_BUTTON_WIDTH,
				HeightRequest = ADD_BUTON_HEIGHT,
				Source = ImageSource.FromFile("tree_add_button.jpg")
			};

			button.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(() => onTap?.Invoke())
			});

			var point = new Point(center.X - ADD_BUTTON_WIDTH / 2, center.Y - ADD_BUTON_HEIGHT / 2);

			return (button, point);
		}
        private (View, Point) GetTreeItem(Point center, Item person, Action onTap = null)
		{
			var element = new CachedImage
			{
				HeightRequest = TREE_ITEM_HEIGHT,
				WidthRequest = TREE_ITEM_WIDTH,
				Aspect = Aspect.AspectFill,
				LoadingPlaceholder = ImageSource.FromFile("no_tree_img"),
				ErrorPlaceholder = ImageSource.FromFile("no_tree_img"),
                Source = person.Image,
				Transformations = new List<FFImageLoading.Work.ITransformation> {
                    new CircleTransformation(BORDER_THICKNESS, person.IsAlive ? "#FFFFFF" : "#000000")
				}
			};

			element.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(() => onTap?.Invoke())
			});

			var point = new Point(center.X - TREE_ITEM_WIDTH / 2, center.Y - TREE_ITEM_HEIGHT / 2);
			return (element, point);
		}

        #endregion
    }
}
