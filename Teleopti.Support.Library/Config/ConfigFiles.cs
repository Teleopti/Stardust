using System;
using System.IO;
using Teleopti.Support.Library.ConfigFiles;

namespace Teleopti.Support.Library.Config
{
	public class ConfigFiles
	{
		private readonly string _file;

		public ConfigFiles(string file)
		{
			_file = file;
		}

		public string[] FileContents()
		{
			var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFiles", _file);
			if (File.Exists(file))
				return File.ReadAllLines(file);
			return new ResourceLoader().Load(_file)
				.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
		}
	}
}