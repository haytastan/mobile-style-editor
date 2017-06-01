﻿
using System;
using mobile_style_editor;
using Xamarin.Forms;

#if __IOS__
using Xamarin.Forms.Platform.iOS;
#elif __ANDROID__
using Xamarin.Forms.Platform.Android;
#elif __UWP__
using Xamarin.Forms.Platform.UWP;
#endif

[assembly: ExportRenderer(typeof(ImageButton), typeof(BaseViewRenderer))]
namespace mobile_style_editor
{
    public class BaseViewRenderer : VisualElementRenderer<RelativeLayout>
	{
		ImageButton View { get; set; }

		protected override void OnElementChanged(ElementChangedEventArgs<RelativeLayout> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				View = e.NewElement as ImageButton;

                Layer.CornerRadius = View.CornerRadius;

                View.CornerRadiusSet += OnCornerRadius;
			}
		}

        void OnCornerRadius(object sender, EventArgs e)
        {
            Layer.CornerRadius = (int)sender;
        }
    }
}
