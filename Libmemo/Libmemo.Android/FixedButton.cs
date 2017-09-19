using System;
using Android.Content.Res;
using Android.Graphics;
using Libmemo;
using Libmemo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(FixedButton), typeof(FixedButtonRenderer))]
namespace Libmemo.Droid
{
    public class FixedButtonRenderer : ButtonRenderer
    {
		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			if (Control != null)
			{
                var newElement = e.NewElement as FixedButton;

				if (newElement != null)
				{
                    var width = newElement.BorderWidth;
                    var color = newElement.BorderColor;
                    var bckColor = newElement.BackgroundColor;

                    var sd = new Android.Graphics.Drawables.ShapeDrawable();
                    sd.Shape = new FilledRectShape(new FilledPaint(){
                        Color = Android.Graphics.Color.Rgb((int)(color.R * 255), (int)(color.G * 255), (int)(color.B * 255)),
                        StrokeWidth = (float)width,
                        FillColor = Android.Graphics.Color.Rgb((int)(bckColor.R * 255), (int)(bckColor.G * 255), (int)(bckColor.B * 255))
                    });
                    Control.Background = sd;
				}
			}
		}
    }

    public class FilledPaint : Paint
    {
        public Android.Graphics.Color FillColor { get; set; }
    }

    public class FilledRectShape : Android.Graphics.Drawables.Shapes.Shape
    {
        public FilledPaint FilledPaint { get; set; }

        public FilledRectShape(FilledPaint paint) : base()
        {
            this.FilledPaint = paint;
        }

        public override void Draw(Canvas canvas, Paint paint)
        {
            var strokeWidth = Android.App.Application.Context.Resources.DisplayMetrics.Density * FilledPaint.StrokeWidth;

            var tmp = new Paint();

            tmp.SetStyle(Paint.Style.Fill);
            tmp.Color = FilledPaint.FillColor;
            canvas.DrawRect(0, 0, canvas.Width, canvas.Height, tmp);

            if (strokeWidth > 0) {
				tmp.SetStyle(Paint.Style.Stroke);
				tmp.StrokeWidth = strokeWidth;
				tmp.Color = FilledPaint.Color;
				canvas.DrawRect(0, 0, canvas.Width, canvas.Height, tmp);          
            }
        }

        protected override void OnResize(float width, float height)
        {
            base.OnResize(width, height);
        }
    }
}