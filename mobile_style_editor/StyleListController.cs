
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Carto.Ui;
using Xamarin.Forms;

#if __IOS__
using Xamarin.Forms.Platform.iOS;
#elif __ANDROID__
using Xamarin.Forms.Platform.Android;
#elif __UWP__
using Xamarin.Forms.Platform.UWP;
#endif

namespace mobile_style_editor
{
    public class StyleListController : BaseController
    {
        public static string GithubOwner = "";
        public static string GithubRepo = "";
        public static string GithubPath = "";
        public static double GithubId = -1;

        public static string CurrentBranch = HubClient.MasterBranch;

        public static string BasePath { get { return (GithubRepo + "/").ToUpper(); } }

        public StyleListView ContentView { get; private set; }

        public StyleListController()
        {
            NavigationPage.SetHasNavigationBar(this, false);

            ContentView = new StyleListView();
            Content = ContentView;

            ContentView.NavigationBar.IsBackButtonVisible = false;
            ContentView.NavigationBar.Title.Text = "CARTO STYLE EDITOR";

#if __IOS__
            DriveClientiOS.Instance.Authenticate();
#endif
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            PopulateTemplateList();

            ContentView.Container.DecelerationEnded += OnScrollViewDecelerationEnd;

            ContentView.AddStyle.Drive.Click += OnDriveButtonClick;
            ContentView.AddStyle.Github.Click += OnGithubButtonClick;

            ContentView.MyStyles.ItemClick += OnStyleClick;
            ContentView.Templates.ItemClick += OnStyleClick;

            ContentView.Templates.RefreshButton.Click += OnTemplateRefreshClick;

            ContentView.Tabs.TabClicked += OnTabClick;

            ContentView.Webview.Authenticated += OnCodeReceived;

            ContentView.FileList.Header.BackButton.Click += OnPopupBackButtonClick;
            ContentView.FileList.Select.Click += OnSelectClick;

            ContentView.FileList.FileContent.ItemClick += OnItemClicked;
            ContentView.FileList.Pages.PageClicked += OnPageClick;

            ContentView.SettingsButton.Click += OnSettingsClick;
            ContentView.Settings.SettingsContent.GithubInfo.LogoutButton.Click += OnLogoutButtonClicked;
            ContentView.Settings.Header.BackButton.Click += OnSettingsBackButtonClick;

            ContentView.FileList.Branches.CellClick += OnBranchCellClicked;

            HubClient.Instance.FileDownloadStarted += OnGithubFileDownloadStarted;


#if __ANDROID__
            DriveClientDroid.Instance.DownloadStarted += OnDownloadStarted;
            DriveClientDroid.Instance.DownloadComplete += OnFileDownloadComplete;
#elif __IOS__
            DriveClientiOS.Instance.DownloadComplete += OnFileDownloadComplete;
            DriveClientiOS.Instance.ListDownloadComplete += OnListDownloadComplete;
#endif

#if __ANDROID__
			ContentView.ShowMapViews();
#endif
            ShowMyStyles();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            ContentView.Container.DecelerationEnded -= OnScrollViewDecelerationEnd;

            ContentView.AddStyle.Drive.Click -= OnDriveButtonClick;
            ContentView.AddStyle.Github.Click -= OnGithubButtonClick;

            ContentView.MyStyles.ItemClick -= OnStyleClick;
            ContentView.Templates.ItemClick -= OnStyleClick;

            ContentView.Templates.RefreshButton.Click -= OnTemplateRefreshClick;

            ContentView.Tabs.TabClicked += OnTabClick;

            ContentView.Webview.Authenticated -= OnCodeReceived;

            ContentView.FileList.Header.BackButton.Click -= OnPopupBackButtonClick;
            ContentView.FileList.Select.Click -= OnSelectClick;

            ContentView.FileList.FileContent.ItemClick -= OnItemClicked;
            ContentView.FileList.Pages.PageClicked -= OnPageClick;

            ContentView.SettingsButton.Click -= OnSettingsClick;
            ContentView.Settings.SettingsContent.GithubInfo.LogoutButton.Click -= OnLogoutButtonClicked;
            ContentView.Settings.Header.BackButton.Click -= OnSettingsBackButtonClick;

            ContentView.FileList.Branches.CellClick -= OnBranchCellClicked;

            HubClient.Instance.FileDownloadStarted -= OnGithubFileDownloadStarted;

#if __ANDROID__
            DriveClientDroid.Instance.DownloadStarted -= OnDownloadStarted;
            DriveClientDroid.Instance.DownloadComplete -= OnFileDownloadComplete;
#elif __IOS__
            DriveClientiOS.Instance.DownloadComplete -= OnFileDownloadComplete;
            DriveClientiOS.Instance.ListDownloadComplete -= OnListDownloadComplete;
#endif
        }

        void OnScrollViewDecelerationEnd(object sender, EventArgs e)
        {
            double x = ContentView.Container.ScrollX;
            double total = ContentView.Container.Width;

            if (x <= total / 2)
            {
				ContentView.Tabs.ScrollToMyStyles();
				ContentView.ScrollTabToMyStyles();
            }
            else
            {
				ContentView.Tabs.ScrollToTemplates();
				ContentView.ScrollTabToTemplates();
            }

            Console.WriteLine("X: " + x);
        }

        async void OnBranchCellClicked(object sender, EventArgs e)
        {
            ContentView.FileList.Branches.Normalize();

            BranchCell cell = (BranchCell)sender;
            cell.Highlight();

            CurrentBranch = cell.Branch.Name;

            var content = await HubClient.Instance.GetRepositoryContent(GithubOwner, GithubRepo, CurrentBranch, GithubPath);
            ContentView.FileList.Show(content.ToGithubFiles());
        }

        async void OnSettingsClick(object sender, EventArgs e)
        {
            bool showing = ContentView.Settings.Toggle();

            if (showing)
            {
                if (HubClient.Instance.IsAuthenticated)
                {
                    ContentView.Settings.SettingsContent.GithubInfo.IsVisible = true;

                    ContentView.Settings.SettingsContent.ShowLoading();

                    Octokit.User user = await HubClient.Instance.GetCurrentUser();
                    ContentView.Settings.SettingsContent.GithubInfo.Update(user);

                    ContentView.Settings.SettingsContent.HideLoading();

                    Stream stream = await HubClient.Instance.GetUserAvatar(user.AvatarUrl);
                    ContentView.Settings.SettingsContent.GithubInfo.Update(stream);
                }
                else
                {
                    ContentView.Settings.SettingsContent.GithubInfo.IsVisible = false;
                    Console.WriteLine("Github not authenticated");
                }
#if __IOS__
                // TODO Get real drive information
                ContentView.Settings.SettingsContent.DriveInfo.Update("Nuti Tab", "nutitab@gmail.com", "icon_avatar_template.png");
#elif __ANDROID__

#endif
            }
        }

        void OnLogoutButtonClicked(object sender, EventArgs e)
        {
            string message = "Are you sure you wish to log out from Github?";
            Alert("", message, null, delegate
            {
                HubClient.Instance.LogOut();
                LocalStorage.Instance.DeleteToken();
                ContentView.Webview.DeleteCookies(HubClient.CookieDomain);
                ContentView.Settings.Hide();
            });
        }

        List<TemplateStyle> templates = new List<TemplateStyle> {
            new TemplateStyle { Name = "voyager.zip" },
            new TemplateStyle { Name = "positron.zip" },
            new TemplateStyle { Name = "darkmatter.zip" }
        };

        async void PopulateTemplateList(bool checkLocal = true)
		{
            await Task.Run(async () =>
            {
                int index = 0;

                List<DownloadResult> results = new List<DownloadResult>();

                foreach (var template in templates)
                {
                    DownloadResult result = await DownloadFile(template.Name, checkLocal);

                    results.Add(result);
                    index++;
                }

                Device.BeginInvokeOnMainThread(delegate
                {
                    ContentView.Templates.ShowStyles(results);
                });
            });
		}

		void InitializeAuthentication()
		{
			GithubAuthenticationData data = HubClient.Instance.PrepareAuthention();
			ContentView.OpenWebviewPopup(data);
		}
                  
        void OnTemplateRefreshClick(object sender, EventArgs e)
        {
            templates = null;
            PopulateTemplateList(false);
        }

		public void OnPageClick(object sender, EventArgs e)
		{
			PageView page = (PageView)sender;
			ContentView.FileList.Show(page.GithubFiles);
		}

		public async void OnCodeReceived(object sender, AuthenticationEventArgs e)
		{
			if (e.IsOk)
			{
				Console.WriteLine("Code: " + e.Code);
				ContentView.Webview.Hide();
				string token = await HubClient.Instance.CreateAccessToken(e.Id, e.Secret, e.Code);
				Console.WriteLine("Token: " + token);

                string message = "Would you like to store your access token so you don't have to log in again?";
                Alert("", message, null, delegate {
                    LocalStorage.Instance.AccessToken = token;
                });
				
				HubClient.Instance.Authenticate(token);
				PopulateTemplateList();

				if (ClickedGithubButton)
				{
					/*
					 * User clicked the button unauthenticated,
					 * went through the whole authentication process,
					 * now repeat step 1 as authenticated user
					 */
					OnGithubButtonClick(null, null);
					ClickedGithubButton = false;
				}
			}
		}

		public void OnTabClick(object sender, EventArgs e)
		{
			ContentView.ScrollTo((StyleTab)sender);
		}

		List<List<GithubFile>> storedContents = new List<List<GithubFile>>();

        protected override bool OnBackButtonPressed()
        {
            if (ContentView.FileList.IsVisible)
            {
                OnPopupBackButtonClick(null, EventArgs.Empty);
                return true;
            }

			return base.OnBackButtonPressed();
        }

        void OnSettingsBackButtonClick(object sender, EventArgs e)
        {
            ContentView.Settings.Hide();
        }

        void OnPopupBackButtonClick(object sender, EventArgs e)
        {
            if (ContentView.Loader.IsRunning)
            {
                return;
            }

            if (storedContents.Count == 0)
            {
                ContentView.FileList.Hide();
                return;
            }

            if (storedContents.Count == 1)
            {
                ContentView.FileList.Pages.Show();
                CurrentBranch = HubClient.MasterBranch;
            }
            
            List<GithubFile> files = storedContents[storedContents.Count - 1];
            ContentView.FileList.Show(files);
            storedContents.Remove(files);

            ContentView.FileList.Header.OnBackPress();

            string[] split = ContentView.FileList.Header.Text.Split('/');

            /*
             * A repository's header text will be "<repository>/", 
             * meaning it will have one '/' character, meaning that split.Length == 2
             * the first item will be the repository name and the second item will be empty
             * 
             */
            bool firstItemValued = !string.IsNullOrWhiteSpace(split[split.Length - 2]);
            bool secondItemEmpty = string.IsNullOrWhiteSpace(split[split.Length - 1]);
            bool isRepository = split.Length == 2 && firstItemValued && secondItemEmpty;

            if (isRepository)
            {
                ContentView.FileList.Branches.IsVisible = true;
                ContentView.FileList.Branches.Highlight(CurrentBranch);
            }
            else
            {
                ContentView.FileList.Branches.IsVisible = false;
            }
        }

		async void OnSelectClick(object sender, EventArgs e)
		{
			if (ContentView.FileList.GithubFiles == null)
			{
				return;
			}

			Device.BeginInvokeOnMainThread(delegate
			{
				ContentView.FileList.Hide();
				ContentView.ShowLoading();
			});

			List<GithubFile> folder = ContentView.FileList.GithubFiles;
            List<DownloadedGithubFile> files = await HubClient.Instance.DownloadFolder(GithubOwner, GithubRepo, CurrentBranch, folder);

			Toast.Show("Saving...", ContentView);

			/*
			 * This is where we update pathing -> Shouldn't use repository folder hierarcy
			 * We get the root of the style folder, and use that as the root folder for our folder hierarcy
			 * 
			 * We can safely assume the root folder of the style, not the repository,
			 * contains a config file (currently only supports project.json)
			 * and therefore can remove any other folder hierarchy
			 * 
			 * TODO What if it always isn't like that? && support other types of config files
			 * 
			 */

			DownloadedGithubFile projectFile = files.Find(file => file.IsProjectFile);

			string folderPath = projectFile.FolderPath;
			string[] split = folderPath.Split('/');

			string rootFolder = split[split.Length - 1];
			int length = folderPath.IndexOf(rootFolder, StringComparison.Ordinal);
			string repoRootFolder = folderPath.Substring(0, length);

			foreach (DownloadedGithubFile file in files)
			{
				file.Path = file.Path.Replace(repoRootFolder, "");
				FileUtils.SaveFileToFolder(file.Stream, file.Path, file.Name);
			}

			string zipname = rootFolder + Parser.ZipExtension;
			string source = Path.Combine(Parser.ApplicationFolder, rootFolder);

			Toast.Show("Comperssing...", ContentView);

			string destination = Parser.Compress(source, zipname, MyStyleFolder);
			// Destination contains filename, just remove it
			destination = destination.Replace(zipname, "");

			Toast.Show("Done!", ContentView);

			Device.BeginInvokeOnMainThread(delegate
			{
				ContentView.HideLoading();
			});

            RepositoryData item = new RepositoryData();

            item.GithubId = GithubId;

            item.StyleName = rootFolder;
            
            item.Owner = GithubOwner;
            item.Name = GithubRepo;
            item.RepositoryPath = GithubPath;
            
            item.Branch = CurrentBranch;
            item.LocalPath = Path.Combine(MyStyleFolder, rootFolder);
            item.CanUploadToGithub = true;

            item.ConstructPrimaryKey();

            bool inserted = LocalStorage.Instance.Insert(item);
            
            // TODO Theoretically should check this earlier to and then prompt 
            // the user whether they wish to overwrite an existing style
            if (!inserted)
            {
                LocalStorage.Instance.Update(item);
            }

			ShowMyStyles();
		}

        void ShowMyStyles()
        {
            Task.Run(delegate
            {
                List<string> paths = FileUtils.GetStylesFromFolder(MyStyleFolder);
                List<DownloadResult> data = FileUtils.GetDataFromPaths(paths);

                Device.BeginInvokeOnMainThread(delegate
                {
                    ContentView.MyStyles.ShowStyles(data);
                });
            });
        }

		public void OnGithubFileDownloadStarted(object sender, EventArgs e)
		{
			string name = (string)sender;
			Toast.Show("Downloading: " + name, ContentView);
		}

		void OnDownloadStarted(object sender, EventArgs e)
		{
			Device.BeginInvokeOnMainThread(delegate
			{
				ContentView.FileList.Hide();
				ContentView.ShowLoading();
			});
		}

		public const string MyStyleFolder = "my-styles";
		public const string TemplateFolder = "template-styles";

		void OnFileDownloadComplete(object sender, DownloadEventArgs e)
		{
			FileUtils.SaveFileToFolder(e.Stream, MyStyleFolder, e.Name);

			Device.BeginInvokeOnMainThread(delegate
			{
				ContentView.HideLoading();
			});

			ShowMyStyles();
		}

		bool ClickedGithubButton;

		int counter = 1;

        async void DownloadRepositories()
		{
			if (counter == 1)
			{
                ContentView.ShowLoading();
			}

			var contents = await HubClient.Instance.GetRepositories(counter);
			ContentView.FileList.Pages.AddPage(contents.ToGithubFiles(), counter);

			if (counter == 1)
			{
				OnListDownloadComplete(null, new ListDownloadEventArgs { GithubFiles = contents.ToGithubFiles() });
				ContentView.FileList.Header.Text = BasePath;
				ContentView.FileList.Pages.HighlightFirst();
			}

			if (contents.Count == HubClient.PageSize)
			{
				counter++;
				DownloadRepositories();
			}
			else
			{
				counter = 1;
			}
		}

		void OnGithubButtonClick(object sender, EventArgs e)
		{
			if (ContentView.Loader != null && ContentView.Loader.IsRunning)
			{
				return;
			}
 			
			if (HubClient.Instance.IsAuthenticated)
			{
                counter = 1;

				ContentView.FileList.Pages.Reset();
				GithubRepo = "";
				storedContents.Clear();
                ContentView.FileList.Pages.Show();
                ContentView.FileList.Branches.IsVisible = false;

				DownloadRepositories();
			}
			else
			{
				ClickedGithubButton = true;
				InitializeAuthentication();
			}
		}

#if __UWP__
		async 
#endif
		void OnDriveButtonClick(object sender, EventArgs e)
		{
#if __ANDROID__
			DriveClientDroid.Instance.Register(Forms.Context);
			DriveClientDroid.Instance.Connect();
#elif __IOS__
			ContentView.ShowLoading();
			DriveClientiOS.Instance.DownloadStyleList();
#elif __UWP__
            ContentView.ShowLoading();
            /*
             * If you crash here, you're probably missing drive_client_ids.json that needs to be bundled as an asset,
             * but because of security reasons, it's not on github.
             * Keys for nutitab@gmail.com are available on Carto's Google Drive under Technology/Product Development/Mobile/keys
             * Simply copy drive_client_ids.json into this project's Assets folder
             * 
             * If you wish to create your ids and refresh tokens,
             * there's a guide under DriveClientiOS
             */
            List<DriveFile> files = await DriveClientUWP.Instance.DownloadFileList();
            OnListDownloadComplete(null, new ListDownloadEventArgs { DriveFiles = files });
#endif
		}

		void OnListDownloadComplete(object sender, ListDownloadEventArgs e)
		{
			Device.BeginInvokeOnMainThread(delegate
			{
				if (e.DriveFiles != null)
				{
					ContentView.FileList.Show(e.DriveFiles);
				}
				else if (e.GithubFiles != null)
				{
					ContentView.FileList.Show(e.GithubFiles);
				}

				ContentView.HideLoading();
			});
		}

		async void OnItemClicked(object sender, EventArgs e)
		{
			FileListPopupItem item = (FileListPopupItem)sender;

			if (!item.IsEnabled)
			{
				Alert("This style is not publicly available and cannot be downloaded");
				return;
			}

			Device.BeginInvokeOnMainThread(delegate
			{
				if (item.GithubFile == null)
				{
					// Do not Hide the popup when dealing with github,
					// as a new page should load almost immediately
					// and we need to show content there as well
					ContentView.FileList.Hide();
				}

				ContentView.ShowLoading();
			});

			if (item.DriveFile != null)
			{
#if __ANDROID__
				/*
				 * Android uses a full-fledged Google Drive component.
				 * No need to manually handle clicks -> Goes straight to FileDownloadComplete()
				 */
#elif __IOS__
				DriveClientiOS.Instance.DownloadStyle(item.DriveFile.Id, item.DriveFile.Name);
#elif __UWP__
                Stream stream = await DriveClientUWP.Instance.DownloadStyle(item.DriveFile.Id, item.DriveFile.Name);

                if (stream == null)
                {
                    Device.BeginInvokeOnMainThread(delegate
                    {
                        ContentView.Popup.Show();
                        ContentView.HideLoading();
                        item.Disable();
                        Alert("This style is not publicly available and cannot be downloaded");
                    });

                } else
                {
                    OnFileDownloadComplete(null, new DownloadEventArgs { Name = item.DriveFile.Name, Stream = stream });
                }
#endif
			}
			else if (item.GithubFile != null)
			{
				/*
				 * Ignore file clicks
				 */
				if (item.GithubFile.IsDirectory)
				{
                    if (item.GithubFile.IsRepository)
                    {
                        GithubOwner = item.GithubFile.Owner;
                        GithubRepo = item.GithubFile.Name;
                        GithubId = item.GithubFile.Id;

                        ContentView.FileList.Pages.Hide();

						ContentView.FileList.Branches.IsVisible = true;
                    }
                    else
                    {
                        ContentView.FileList.Branches.IsVisible = false;
                    }

					GithubPath = item.GithubFile.Path;
					await LoadGithubContents();

                    if (item.GithubFile.IsRepository)
                    {
                        var branches = await HubClient.Instance.GetBranches(GithubOwner, GithubRepo);
                        ContentView.FileList.Branches.Add(branches);

                        ContentView.FileList.Branches.Highlight(CurrentBranch);
                    }
				}
			}
		}

		async Task<bool> LoadGithubContents()
		{
			storedContents.Add(ContentView.FileList.GithubFiles);

			if (GithubPath == null)
			{
				// Path will be null if we're dealing with a repository
				GithubPath = "";
			}

 			var contents = await HubClient.Instance.GetRepositoryContent(GithubOwner, GithubRepo, CurrentBranch, GithubPath);
			ContentView.FileList.Show(contents.ToGithubFiles());
			ContentView.HideLoading();

			Device.BeginInvokeOnMainThread(delegate
			{
				ContentView.FileList.Header.Text = BasePath + GithubPath.ToUpper();
			});

			return true;
		}

		public
#if __UWP__
		async 
#endif
		void Alert(string message)
		{
#if __UWP__
            var dialog = new Windows.UI.Popups.MessageDialog(message);
            await dialog.ShowAsync();
#endif
		}

		void OnStyleClick(object sender, EventArgs e)
		{
			StyleListItem item = (StyleListItem)sender;

			Device.BeginInvokeOnMainThread(async delegate
			{
				await Navigation.PushAsync(new MainController(item.Data.Path, item.Data.Filename));
#if __ANDROID__
				ContentView.HideMapViews();
#endif
			});
		}

        public async Task<DownloadResult> DownloadFile(string name, bool checkLocal = true)
        {
            bool existsLocally = true;

            if (checkLocal)
            {
                existsLocally = FileUtils.HasLocalCopy(TemplateFolder, name);
            }
            else
            {
                existsLocally = false;
            }

            string path;
            string filename;

            if (!existsLocally)
            {
                var stream = await Networking.GetStyle(name);
                List<string> data = FileUtils.SaveFileToFolder(stream, TemplateFolder, name);

                path = data[1];
                filename = data[0];
            }
            else
            {
                path = FileUtils.GetLocalPath(TemplateFolder);
                filename =name;
            }

            return new DownloadResult { Path = path, Filename = filename };
        }

	}
}
