﻿using System;
using Octokit;

namespace mobile_style_editor
{
	public class GithubFile
	{
		public const string PROJECTFILE = "project.json";

		public string Name { get; set; }

		public string Path { get; set; }

		public string Sha { get; set; }

		public bool IsDirectory { get; set; }

		public string Extension { get; set; }

		public string DownloadUrl { get; set; }

		public bool IsZip { get { return Extension.Equals("zip"); } }

		internal static GithubFile FromRepositoryContent(RepositoryContent file)
		{
			GithubFile item = new GithubFile();
			item.Name = file.Name;
			item.Path = file.Path;
			item.Sha = file.Sha;
			item.IsDirectory = (file.Type == ContentType.Dir);
			if (!item.IsDirectory)
			{
				item.DownloadUrl = file.DownloadUrl.OriginalString;
			}
			string[] split = file.Name.Split('.');
			item.Extension = split[split.Length - 1];

			return item;
		}

		public bool IsProjectFile
		{
			get { return Name.Equals(PROJECTFILE); }
		}
	}
}