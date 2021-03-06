﻿
using System;
using System.Collections.Generic;
#if __UWP__
#else
using SQLite;
#endif
using Xamarin.Forms;

namespace mobile_style_editor
{
    public class LocalStorage
    {
        public static LocalStorage Instance = new LocalStorage();

        SQLiteConnection db;

        LocalStorage()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            const string file = "carto_style_editor.db";

            db = new SQLiteConnection(System.IO.Path.Combine(folder, file));

            //RefreshTables();
            db.CreateTable<RepositoryData>();
        }

        void RefreshTables()
        {
            List<RepositoryData> data = db.Query<RepositoryData>("SELECT * from RepositoryData");

            db.DropTable<RepositoryData>();
            db.CreateTable<RepositoryData>();

            db.InsertAll(data);

        }

        #region Github Access Token

        const string ACCESSTOKEN = "access_token";
        const string POPUPSHOWN = "popup_shown";

        public bool HasAccessToken
        {
            get { return Application.Current.Properties.ContainsKey(ACCESSTOKEN); }
        }

        public string AccessToken
        {
            get { return (string)Application.Current.Properties[ACCESSTOKEN]; }
            set
            {
                if (!HasAccessToken)
                {
                    Application.Current.Properties.Add(ACCESSTOKEN, value);
                }
                else
                {
                    Application.Current.Properties[ACCESSTOKEN] = value;
                }

                Application.Current.SavePropertiesAsync();
            }
        }

        public void DeleteToken()
        {
            Application.Current.Properties.Remove(ACCESSTOKEN);
            Application.Current.SavePropertiesAsync();
        }
        #endregion

        public bool WarningPopupShown
        {
            get { return Application.Current.Properties.ContainsKey(POPUPSHOWN) && (bool)Application.Current.Properties[POPUPSHOWN] == true; }
            set
            {
				if (!WarningPopupShown)
				{
					Application.Current.Properties.Add(POPUPSHOWN, value);
				}
				else
				{
					Application.Current.Properties[POPUPSHOWN] = value;
				}

				Application.Current.SavePropertiesAsync();
            }
        }

        #region RepositoryData

        public bool Insert(RepositoryData item)
        {
            try
            {
                db.Insert(item);
                return true;
            } catch
            {
                return false;    
            }
        }

        public void Update(RepositoryData item)
        {
            db.Update(item);
        }

        public RepositoryData GetRepositoryData(string localPath)
        {
            var list = db.Query<RepositoryData>("SELECT * FROM RepositoryData Where LocalPath = '" +  localPath + "'");

            if (list.Count != 1)
            {
                return new RepositoryData();
            }

            return list[0];
        }

        #endregion

    }
}
