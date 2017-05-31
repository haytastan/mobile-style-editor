﻿
using System;
using Xamarin.Forms;

namespace mobile_style_editor
{
    public class UserInfo : BaseView
    {
		Image image;
        Label header, login, name;

        public LogoutButton LogoutButton { get; private set; }

        public UserInfo()
        {
            header = new Label();
            header.TextColor = Colors.CartoNavy;
            header.FontSize = 12;

            image = new Image();

            login = new Label();
            login.FontAttributes = FontAttributes.Bold;
            login.FontSize = 15;
            login.VerticalTextAlignment = TextAlignment.Center;

            name = new Label();
            name.TextColor = Color.FromRgb(100, 100, 100);
            name.FontSize = 12;

            LogoutButton = new LogoutButton();
        }

        public override void LayoutSubviews()
        {
            double padding = Height / 13;

            double x = padding;
            double y = padding;
            double w = Width - 2 * padding;
            double h = 20;

            AddSubview(header, x, y, w, h);

            y += h;

            w = Height - (3 * padding + h);
            h = w;

            AddSubview(image, x, y, w, h);

            x += w + padding;

            w = Width - (3 * padding + h);
            h = 20;

            AddSubview(login, x, y, w, h);

            y += h;
            h = 15;

            AddSubview(name, x, y, w, h);

            w = 100;
            h = w / 3;
            x = Width - (w + padding);
            y = Height - (h + padding);

            AddSubview(LogoutButton, x, y, w, h);
        }

        public void Update(Octokit.User user)
        {
            login.Text = user.Login;
            name.Text = user.Name;

			header.Text = "LOGGED INTO GITHUB AS";
        }

        public void Update(System.IO.Stream stream)
        {
            image.Source = ImageSource.FromStream(() => stream);
        }
    }

    public class LogoutButton : ClickView
    {
        Image image;
        Label text;

        public LogoutButton()
        {
            image = new Image();
            image.Source = ImageSource.FromFile("icon_logout.png");

            text = new Label();
            text.Text = "LOG OUT";
            text.VerticalTextAlignment = TextAlignment.Center;
            text.HorizontalTextAlignment = TextAlignment.Center;
            text.TextColor = Colors.CartoNavy;
            text.FontAttributes = FontAttributes.Bold;
            text.FontSize = 13;
        }

        public override void LayoutSubviews()
        {
            double padding = Height / 10;
            double imageSize = Height - 2 * padding;

            double x = 0;
            double y = 0;
            double w = Width - (imageSize + 2 * padding);
            double h = Height;

            AddSubview(text, x, y, w, h);

            x += w + padding;
            y = padding;
            w = imageSize;
            h = imageSize;

            AddSubview(image, x, y, w, h);
        }
    }
}
