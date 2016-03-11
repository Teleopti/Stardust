using System;
using System.IO;

namespace Teleopti.Support.Tool.Tool
{
	public class ModeFile
	{

		public string Type { get; private set; }

		public ModeFile(string type)
		{
			Type = type;
		}

		public string[] FileContents()
		{
			var file = "ConfigFiles.txt";
			if (Type.Equals("DEPLOY",StringComparison.InvariantCultureIgnoreCase))
				file = "DeployConfigFiles.txt";
			if (Type.Equals("TEST",StringComparison.InvariantCultureIgnoreCase))
				file = "BuildServerConfigFiles.txt";
			if (Type.Equals("AZURE",StringComparison.InvariantCultureIgnoreCase))
				file = "AzureConfigFiles.txt";

			return File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ConfigFiles\" + file));
		}
	}
}