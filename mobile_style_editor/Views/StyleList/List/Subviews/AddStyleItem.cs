﻿using System;
using Xamarin.Forms;

namespace mobile_style_editor
{
	public class AddStyleItem : BaseView
	{
		BaseView container;

		Label titleLabel;
		BaseView separator;

		public PickerViewItem Github { get; private set; }
		public PickerViewItem Drive { get; private set; }

		public AddStyleItem()
		{
			BorderWidth = 1;
			BorderColor = Colors.CartoNavyLight;
			//Elevated = true;
            BackgroundColor = StyleListView.Background;

			container = new BaseView();
			
			titleLabel = new Label();
            titleLabel.TextColor = Colors.CartoNavy;
			titleLabel.FontSize = 13;
            titleLabel.FontAttributes = FontAttributes.None;

			titleLabel.VerticalTextAlignment = TextAlignment.Center;
			titleLabel.Text = "ADD STYLE";

			separator = new BaseView();
            separator.BackgroundColor = Colors.CartoNavy;

            string folder = "";

#if __UWP__
            folder = "Assets/";
#endif
			Github = new PickerViewItem(folder + "icon_github.png", "GITHUB");
			Github.TextSize = 10;

			Drive = new PickerViewItem(folder + "icon_drive.png", "GOOGLE DRIVE");
			Drive.TextSize = 10;

            Color buttonColor = Color.White;
			Github.BackgroundColor = buttonColor;
			Drive.BackgroundColor = buttonColor;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			double padding = 5;

			AddSubview(container, padding, padding, Width - 2 * padding, Height - 2 * padding);

			double titleHeight = container.Height / 5;

			double x = padding;
			double y = 0;
			double w = container.Width - 2 * padding;
			double h = titleHeight;

			container.AddSubview(titleLabel, x, y, w, h);

			double separatorW = container.Width / 4 * 3;
			x = container.Width / 2 - separatorW / 2;
			y = h - 2;
			w = separatorW;
			h = 1;

			container.AddSubview(separator, x, y, w, h);

			double itemSize = container.Height - (titleHeight + 2 * padding);
			double itemPadding = 10;

			// PickerViewItem count
			int count =  2;

			h = itemSize;
			w = itemSize;
			x = container.Width - (count* itemSize + count* itemPadding);
			y = titleHeight + padding;

			container.AddSubview(Github, x, y, w, h);

			x += itemSize + itemPadding;

			container.AddSubview(Drive, x, y, w, h);
		}
	}
}
