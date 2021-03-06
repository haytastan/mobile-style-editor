﻿using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace mobile_style_editor
{
    public class StyleContainer : BaseView
    {
        public EventHandler<EventArgs> ItemClick;

        public bool ContainsRefreshButton { get; set; }
        public RefreshButton RefreshButton { get; private set; }

		public BaseView Header { get; set; }

		BaseView separator;

		BaseScrollView styleList;

		public BaseView Footer { get; set; }

		public List<StyleListItem> Items
		{
			get {
				return styleList.Children.Where(child => child is StyleListItem).Cast<StyleListItem>().ToList();
			}
		}

		public StyleContainer()
		{
			styleList = new BaseScrollView();

            separator = new BaseView { BackgroundColor = Color.Black };

            RefreshButton = new RefreshButton();
            RefreshButton.IsVisible = false;
		}

		double padding = 5;

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			double headerHeight = Height > Width ? Height / 17 : Width / 17;
			double headerPadding = headerHeight / 4;

			double x = padding;
			double y = 0;
			double w = Width - 2 * padding;
			double h = headerHeight;

            if (Header != null)
            {
                AddSubview(Header, x, y, w, h);

                // Separator
                y += h + 3;
                h = 1;
                AddSubview(separator, 2 * padding, y + h, w - 2 * padding, h);
                y += headerPadding;
            }

            y += h + headerPadding;

			// For some reason listview is too short, substracting a random constant: 15
			h = Height - (headerHeight + headerPadding + 15);

			AddSubview(styleList, x, y, w, h);

			UpdateListLayout();

            if (ContainsRefreshButton)
            {
                w = 30;
                h = w;
                x = Width - (w + padding);
                y = padding;

                AddSubview(RefreshButton, x, y, w, h);
                RaiseChild(RefreshButton);
            }
		}

		public void RenderList(List<Octokit.RepositoryContent> contents)
		{
			styleList.Clear();

			foreach (var content in contents)
			{
				var item = new StyleListItem();

				item.Click += (object sender, EventArgs e) =>
				{
					if (ItemClick != null)
					{
						ItemClick(sender, e);
					}
				};

				styleList.AddSubview(item);

				item.Update(content);
			}

			UpdateListLayout();
		}

        public bool RenderMap(DownloadResult result)
        {
            var item = Items.Find(i => i.Data.Filename == result.Filename && i.Data.Path == result.Path);

            if (item != null)
            {
                item.Update(result);
                return true;
            }

            return false;
        }

		public void RenderMap(DownloadResult result, int index)
		{
            StyleListItem item = Items[index];
            item.Update(result);
        }

		public void ShowStyles(List<DownloadResult> results)
		{
			styleList.Clear();	

			foreach (DownloadResult result in results)
			{
				var item = new StyleListItem();

				item.Click += (object sender, EventArgs e) =>
				{
					if (ItemClick != null)
					{
						ItemClick(sender, e);
					}
				};

				styleList.AddSubview(item);

				item.Update(result);
			}

			UpdateListLayout();
		}

		void UpdateListLayout()
		{
			double x = 0;
			double y = 0;
			double w = styleList.Width;
			double h = 150;

			foreach (var child in styleList.Children)
			{
				if (child is StyleListItem)
				{
					styleList.AddSubview(child, x, y, w, h);
					y += h + padding;
				}
			}

            if (Footer != null)
            {
                styleList.AddSubview(Footer, x, y, w, h);
            }
        }

		public static BaseView GetHeaderLabel(string text)
		{
			BaseView view = new BaseView();
			view.ClearChildrenOnLayout = false;

			Label label = new Label();
			label.Text = text;
			label.VerticalTextAlignment = TextAlignment.End;
            label.TextColor = Color.Black;
			label.Margin = new Thickness(0, 0);
			label.FontAttributes = FontAttributes.Bold;
			label.FontSize = 15f;

			view.AddSubview(label, 0, 0, BaseView.MatchParent, BaseView.MatchParent);

			return view;
		}

	}
}
