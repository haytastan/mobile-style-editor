using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#if __ANDROID__
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Drive;
using Android.Gms.Drive.Query;
using Android.OS;
using Android.Runtime;
#elif __IOS__
#endif

namespace mobile_style_editor
{

	public class DriveClient
#if __ANDROID__
	 : Java.Lang.Object, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
#elif __IOS__

#endif
	{
		public static DriveClient Instance = new DriveClient();

		/*
		 * Requires activity reference. Thankfully, Xamarin.Forms.Forms.Context is the default MainActivity,
		 * when allowed, the activity's OnActivityResult will be called
		 * 
		 * Be sure to use the correct keystore's SHA1 when registering a client id,
		 * else it'll just return "Canceled" (0) in OnActivityResult without any additional error message.
		 * cf. https://developer.xamarin.com/guides/android/deployment,_testing,_and_metrics/MD5_SHA1/#OSX for Xamarin defaults
		 * 
		 */

		public const int RequestCode_RESOLUTION = 1;
		public const int RequestCode_OPENER = 2;

		public const string Response_DRIVEID = "response_drive_id";

		public EventHandler<DownloadEventArgs> DownloadComplete;

#if __ANDROID__
		Context context;

		GoogleApiClient client;

		public bool IsConnecting { get { return client.IsConnecting; } }

		public void Register(Context context)
		{
			this.context = context;

			GoogleApiClient.Builder builder = new GoogleApiClient.Builder(context, this, this);
			builder.AddApi(DriveClass.API);
			builder.AddScope(DriveClass.ScopeFile);

			client = builder.Build();
		}

		public void Upload(string currentWorkingName, MemoryStream currentWorkingStream)
		{
			Task.Run(delegate
			{
				currentWorkingStream.Seek(0, SeekOrigin.Begin);
				var result = DriveClass.DriveApi.NewDriveContents(client).Await().JavaCast<IDriveApiDriveContentsResult>();

				var metaBuilder = new MetadataChangeSet.Builder();
				metaBuilder.SetMimeType(Type_Zip);
				metaBuilder.SetTitle(currentWorkingName);

				using (StreamWriter writer = new StreamWriter(result.DriveContents.OutputStream))
				{
					writer.Write(currentWorkingStream);
					writer.Close();
				}

				//CreateFileActivityBuilder fileBuilder = DriveClass.DriveApi.NewCreateFileActivityBuilder();
				//fileBuilder.SetInitialMetadata(metaBuilder.Build());

				DriveClass.DriveApi.GetRootFolder(client).CreateFile(client, metaBuilder.Build(), result.DriveContents);
			});
		}

		public void Connect()
		{
			client.Connect();
		}

		public void OnConnected(Bundle connectionHint)
		{
			OpenPicker();
		}

		const string Type_Zip = "application/zip";
		static readonly string[] MimeType = { Type_Zip };

		public void OpenPicker()
		{
			OpenFileActivityBuilder builder = DriveClass.DriveApi.NewOpenFileActivityBuilder();
			builder.SetMimeType(MimeType);

			((Activity)context).StartIntentSenderForResult(builder.Build(client), RequestCode_OPENER, null, 0, 0, 0);
		}

		public void Download(DriveId driveId)
		{
			/*
			 * Download snippet from:
			 * http://stackoverflow.com/questions/37407368/android-drive-api-download-file
			 */

			Task.Run(delegate
			{
				/*
				 * All "Result" variables are under Android.Gms.Drive, search for "result".
				 * Be sure to await, else it'll return cast failure
				 */

				IDriveFile file = DriveClass.DriveApi.GetFile(client, driveId);
				IDriveApiDriveContentsResult result = file.Open(client, Android.Gms.Drive.DriveFile.ModeReadOnly, null).Await().JavaCast<IDriveApiDriveContentsResult>();

				IDriveResourceMetadataResult metadataResult = file.GetMetadata(client).Await().JavaCast<IDriveResourceMetadataResult>();
				
				Stream stream = result.DriveContents.InputStream;

				if (DownloadComplete != null)
				{
					string name = metadataResult.Metadata.Title;
					DownloadComplete(null, new DownloadEventArgs { Stream = stream, Name = name });
				}
			});
		}

		public void OnConnectionFailed(ConnectionResult result)
		{
			if (result.HasResolution)
			{
				try
				{
					result.StartResolutionForResult((Activity)context, RequestCode_RESOLUTION);
				}
				catch (IntentSender.SendIntentException e)
				{
					Console.WriteLine("Failed to start resolution result: " + e.Message);
				}
			}
			else
			{
				Console.WriteLine("Failed without resolution");
			}
		}

		public void OnConnectionSuspended(int cause)
		{
			// TODO When is this called?
			throw new NotImplementedException();
		}

#elif __IOS__

		public void Register()
		{

		}

		public void Connect()
		{
			
		}
#endif
	}
}

// TODO REMOVE old, unused logic
//	public class QueryCallback : Java.Lang.Object, IResultCallback
//	{
//		public void OnResult(Java.Lang.Object result)
//		{
//			IDriveApiMetadataBufferResult parsed = result.JavaCast<IDriveApiMetadataBufferResult>();
//			Console.WriteLine(parsed);
//		}
//	}

//	public class FetchIdCallback : Java.Lang.Object, IResultCallback
//	{
//		public void OnResult(Java.Lang.Object result)
//		{
//			IDriveApiDriveIdResult parsed = result.JavaCast<IDriveApiDriveIdResult>();
//			Console.WriteLine(parsed);
//		}
//	}

//			// Method for creating a folder. Non-functional
//			MetadataChangeSet.Builder changeset = new MetadataChangeSet.Builder();
//	changeset.SetTitle("testTitle");
//			changeset.SetDescription("testDescription");

//			IDriveFolder appFolder = DriveClass.DriveApi.GetAppFolder(client);
//			if (appFolder != null)
//			{
//				await appFolder.CreateFolder(client, changeset.Build());
//			}
//			else
//			{
//				Console.WriteLine(":( appFolder is still null");
//			}

//			QueryClass query = new QueryClass.Builder().Build();

//// Method 1 for getting Drive files/folders
//DriveClass.DriveApi.Query(client, query).SetResultCallback(new QueryCallback());

//			// Method 2 for getting Drive files/folders
//			IDriveFolder folder = DriveClass.DriveApi.GetRootFolder(client);
//IDriveApiMetadataBufferResult result1 = (await folder.ListChildren(client)).JavaCast<IDriveApiMetadataBufferResult>();
//IEnumerator<Metadata> list1 = result1.MetadataBuffer.GetEnumerator();

//			while (list1.MoveNext())
//			{
//				Metadata current = list1.Current;
//Console.WriteLine(current);
//			}

//			// Method 3 for getting Drive files/folders
//			IDriveApiMetadataBufferResult result2 = await DriveClass.DriveApi.QueryAsync(client, query);
//IEnumerator<Metadata> list2 = result2.MetadataBuffer.GetEnumerator();

//			while (list2.MoveNext())
//			{
//				Metadata current = list2.Current;
//Console.WriteLine(current);
//			}
