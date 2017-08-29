using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Libmemo.Helpers
{
    public class Tree
    {
        private AbsoluteLayout Layout { get; set; }
        private ScrollView Scroll { get; set; }
        public Item.RootItem Root { get; set; }

		public abstract class Item
		{
			public int Id { get; set; }
			public string Fio { get; set; }
			public ImageSource Image;

            public Item(int id, string fio, string image_uri = null)
            {
                Id = id;
                Fio = fio;
                Image = image_uri != null && Uri.TryCreate(image_uri, UriKind.Absolute, out Uri uri)
                    ? ImageSource.FromUri(uri)
                    : ImageSource.FromFile("");
            }

			public abstract class ItemWithRelatives : Item
			{
				public ItemWithRelatives(int id, string fio, string image_uri = null) : base(id, fio, image_uri) { }

				public NormalItem Mother { get; set; }
				public NormalItem Father { get; set; }
				public List<SiblingItem> Siblings { get; set; } = new List<SiblingItem>();
			}

			public class SiblingItem : Item
			{
				public SiblingItem(int id, string fio, string image_uri = null) : base(id, fio, image_uri) { }
			}


            public class NormalItem : ItemWithRelatives
            {
                public NormalItem(int id, string fio, string image_uri = null) : base(id, fio, image_uri) {}
            }

			public class RootItem : ItemWithRelatives
			{
				public RootItem(int id, string fio, string image_uri = null) : base(id, fio, image_uri) { }
			}

		}


        public Tree(AbsoluteLayout absolute, ScrollView scroll, Json.Tree json)
		{
			this.Layout = absolute;
			this.Scroll = scroll;
		}





    }
}
