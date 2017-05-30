﻿using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace mobile_style_editor
{
	public class FileListPopup : BasePopup
	{
		public FileListPopupContent FileContent { get { return Content as FileListPopupContent; } }

		public FileListHeader Header { get; private set; }

		public PaginationView Pages { get; private set; }

		public FileListPopup()
		{
			Content = new FileListPopupContent();

			Header = new FileListHeader();

			Pages = new PaginationView();
			Pages.ContentHeight = 40;

			base.Hide(false);
		}

		public override void LayoutSubviews()
		{
			double x = ContentX;
			double y = ContentY;
			double w = ContentWidth;
			double h = ContentHeight;

			AddSubview(Content, x, y, w, h);

			double padding = 10;

			w = ContentWidth;
			h = VerticalPadding - 3 * padding;
			x = HorizontalPadding;
			y = VerticalPadding - h;

			AddSubview(Header, x, y, w, h);

			w = ContentWidth;
			h = Pages.ContentHeight;
			x = ContentX;
			y = ContentY + ContentHeight;

			AddSubview(Pages, x, y, w, h);
		}

		public void Show(List<DriveFile> files)
		{
			Show();
			FileContent.Populate(files.ToObjects());
			Header.IsVisible = false;
		}

		public void Show(List<StoredStyle> styles)
		{
			Show();
			FileContent.Populate(styles.ToObjects());
		}

		public List<GithubFile> GithubFiles { get; private set; }

		public void Show(List<GithubFile> files)
		{
			Header.IsVisible = true;

			Show();
			GithubFiles = files;
			FileContent.Populate(files.ToObjects());

			if (files.Any(file => file.IsProjectFile))
			{
				Header.Select.Enable();
			}
			else
			{
				Header.Select.Disable();
			}
		}

		public override async void Hide(bool animated = true)
		{
			if (Pages != null)
			{
				Pages.Reset();
			}
			base.Hide(animated);
		}

	}
}
