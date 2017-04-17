﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Carto.Core;
using Foundation;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using UIKit;

namespace mobile_style_editor.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();

			LoadApplication(new EditorApplication());

            AuthenticateDrive();

			return base.FinishedLaunching(app, options);
		}

		const string CLIENTID_KEY = "client_id";
		const string CLIENTSECRET_KEY = "client_secret";
		const string REFRESHTOKEN_KEY = "refresh_token";

		static string RefreshToken = "";
		static string ClientId = "";
		static string ClientSecret = "";

		/*
		 * (1) Register "Other" application to get both clientId and clientSecret
         * Note that official guides are of no use, 
         * i.e. https://developers.google.com/drive/v3/web/quickstart/dotnet sample just causes the application to freeze;
         * Nothing is returned. Found alternative means:
         * http://stackoverflow.com/questions/27573272/googlewebauthorizationbroker-authorizeasync-hangs
		 * 
		 * (2) To get correct authentication token, follow the guide at:
         * http://stackoverflow.com/questions/5850287/youtube-api-single-user-scenario-with-oauth-uploading-videos/8876027#8876027
         *
		 * Finally:
		 * Constructed.json file for in-house use. Contect aare@carto.com to get a hold of file drive_client_ids.json,
		 * otherwise you need to go through the entire application registration process
		 * 
		 * Milestone TODO (probably not worth it to implement in the near future):
		 * Allow the user to authenticate via basic webview, HTTPRequest to retrieve refresh_token after that
		 */

		void AuthenticateDrive()
		{
			using (var stream = new FileStream("drive_client_ids.json", FileMode.Open, FileAccess.Read))
			{
				using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
			    {
					Variant data = Variant.FromString(reader.ReadToEnd());

					ClientId = data.GetObjectElement(CLIENTID_KEY).String;
					ClientSecret = data.GetObjectElement(CLIENTSECRET_KEY).String;
					RefreshToken = data.GetObjectElement(REFRESHTOKEN_KEY).String;
			    }	
			}

			var secrets = new ClientSecrets() { ClientId = ClientId, ClientSecret = ClientSecret };
			var initializer = new GoogleAuthorizationCodeFlow.Initializer { ClientSecrets = secrets };
			var flow = new GoogleAuthorizationCodeFlow(initializer);

			var token = new TokenResponse { RefreshToken = RefreshToken };

			var credentials = new UserCredential(flow, "user", token);
			
			var service = new DriveService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credentials,
				/* ApplicationName doesn't necessarily have to be package id / bundle identifier */
				ApplicationName = "com.carto.style.editor",
			});

			// Define parameters of request.
			FilesResource.ListRequest listRequest = service.Files.List();
			listRequest.PageSize = 10;
			listRequest.Fields = "nextPageToken, files(id, name)";

			// List files.
			IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
			Console.WriteLine("Files:");
			if (files != null && files.Count > 0)
			{
				int counter = 1;

				foreach (var file in files)
				{
					Console.WriteLine(counter + ". {0} ({1})", file.Name, file.Id);
					counter++;
				}
			}
			else
			{
				Console.WriteLine("No files found.");
			}
		}
	}
}
