using System;
using System.Globalization;
using System.IO;

namespace Teleopti.Support.Code.Tool
{
	[CLSCompliant(true)]
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
			if (Type.ToUpper(CultureInfo.InvariantCulture).Equals("DEPLOY"))
				file = "DeployConfigFiles.txt";
			if (Type.ToUpper(CultureInfo.InvariantCulture).Equals("TEST"))
				file = "BuildServerConfigFiles.txt";
			if (Type.ToUpper(CultureInfo.InvariantCulture).Equals("AZURE"))
				file = "AzureConfigFiles.txt";

			return File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ConfigFiles\" + file));
		}
	}
}