using System;
using System.IO;
using Teleopti.Support.Library.Folders;

namespace Teleopti.Support.Library.Config
{
	public class RefreshConfigFile
	{
		private readonly FileConfigurator _fileConfigurator;
		private readonly RepositoryRootFolder _repositortRootFolder;
		private readonly string _buildArtifacts;

		public RefreshConfigFile()
		{
			_fileConfigurator = new FileConfigurator();
			_repositortRootFolder = new RepositoryRootFolder();
			_buildArtifacts = new BuildArtifactsFolder().Path();
		}

		public void ReplaceFile(string destinationAndSource, SearchReplaceCollection searchReplaces)
		{
			var files = destinationAndSource.Split(',');
			if (files.Length.Equals(2))
				ReplaceFile(files[0], files[1], searchReplaces);
		}

		public void ReplaceFile(string destinationFile, string sourceFile, SearchReplaceCollection searchReplaces)
		{
			sourceFile = MakePath(sourceFile);
			destinationFile = MakePath(destinationFile);

			var dir = GetDirectories(destinationFile);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			_fileConfigurator.Configure(sourceFile, destinationFile, searchReplaces);
		}

		public string MakePath(string file)
		{
			if (file.StartsWith("$"))
			{
				if (_repositortRootFolder.IsRunningFromRepository())
					file = file.Replace("$(Repository)", $@"{_repositortRootFolder.Path()}\");
				file = file.Replace("$(BuildArtifacts)", $@"{_buildArtifacts}\");
				file = file.Replace("$(SupportTool)", $@"{AppDomain.CurrentDomain.BaseDirectory}\");
				return file;
			}

			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
		}

		public string GetDirectories(string fullPath)
		{
			var pos = fullPath.LastIndexOf(@"\", StringComparison.InvariantCulture);
			if (pos == -1) return @".";
			return fullPath.Substring(0, pos);
		}
	}
}