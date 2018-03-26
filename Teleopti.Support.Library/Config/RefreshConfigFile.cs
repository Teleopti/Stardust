using System;
using System.IO;

namespace Teleopti.Support.Library.Config
{
	public class RefreshConfigFile
	{
		private readonly FileConfigurator _fileConfigurator;

		public RefreshConfigFile()
		{
			_fileConfigurator = new FileConfigurator();
		}

		public void ReplaceFile(string destinationAndSource, SearchReplaceCollection searchReplaces)
		{
			var files = destinationAndSource.Split(',');
			if (files.Length.Equals(2))
				ReplaceFile(files[0], files[1], searchReplaces);
		}

		public void ReplaceFile(string destinationFile, string sourceFile, SearchReplaceCollection searchReplaces)
		{
			destinationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, destinationFile);
			sourceFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sourceFile);

			var dir = GetDirectories(destinationFile);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			_fileConfigurator.Configure(sourceFile, destinationFile, searchReplaces);
		}

		public string GetDirectories(string fullPath)
		{
			var pos = fullPath.LastIndexOf(@"\", StringComparison.InvariantCulture);
			if (pos == -1) return @".";
			return fullPath.Substring(0, pos);
		}
	}
}