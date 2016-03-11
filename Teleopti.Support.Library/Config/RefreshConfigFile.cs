using System;
using System.IO;

namespace Teleopti.Support.Library.Config
{
	public interface IRefreshConfigFile
	{
		void ReplaceFile(string destinationAndSource, SearchReplaceCollection searchReplaceCollection);
	}

	public class RefreshConfigFile : IRefreshConfigFile
	{
		private readonly FileConfigurator _fileConfigurator;
		private readonly IMachineKeyChecker _machineKeyChecker;

		public RefreshConfigFile()
		{
			_fileConfigurator = new FileConfigurator();
			_machineKeyChecker = new MachineKeyChecker();
		}

		public void ReplaceFile(string destinationAndSource, SearchReplaceCollection searchReplaceCollection)
		{
			var files = destinationAndSource.Split(',');
			if (files.Length.Equals(2))
				ReplaceFile(files[0], files[1], searchReplaceCollection);
		}

		public void ReplaceFile(string destinationFile, string sourceFile, SearchReplaceCollection searchReplaceCollection)
		{
			destinationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, destinationFile);
			sourceFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sourceFile);

			var dir = GetDirectories(destinationFile);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			_fileConfigurator.Configure(sourceFile, destinationFile, searchReplaceCollection);
			_machineKeyChecker.CheckForMachineKey(destinationFile);
		}

		public string GetDirectories(string fullPath)
		{
			var pos = fullPath.LastIndexOf(@"\", StringComparison.InvariantCulture);
			if (pos == -1) return @".";
			return fullPath.Substring(0, pos);
		}
	}

}