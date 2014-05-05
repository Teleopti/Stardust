using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;

namespace Teleopti.Support.Code.Tool
{
    public interface ICommandLineArgument
    {
		ModeFile Mode { get; }
        string Help { get; }
        bool ShowHelp { get; }
    }

	public class ModeFile
	{
		private readonly string _mode;

		public ModeFile(string mode)
		{
			_mode = mode;
		}

		public string[] FileContents()
		{
			var file = "ConfigFiles.txt";
			if (_mode.ToUpper(CultureInfo.InvariantCulture).Equals("DEPLOY"))
				file = "DeployConfigFiles.txt";
			if (_mode.ToUpper(CultureInfo.InvariantCulture).Equals("TEST"))
				file = "BuildServerConfigFiles.txt";
			if (_mode.ToUpper(CultureInfo.InvariantCulture).Equals("AZURE"))
				file = "AzureConfigFiles.txt";

			return File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ConfigFiles\" + file));
		}
	}

    public class CommandLineArgument : ICommandLineArgument
    {
       private readonly string[] _argumentCollection;
        
        public CommandLineArgument(string[] argumentCollection)
        {
            _argumentCollection = argumentCollection;
            ReadArguments();
        }

		public ModeFile Mode { get; private set; }
		public bool SaveSsoConfig { get; set; }
		public bool ShowHelp { get; private set; }
        public string Help
        {
            get { return @"Command line arguments:

        -? or ? or -HELP or HELP, Shows this help
        -MO  (Mode )is where to put the config files values: Develop or Deploy (Develop is default)";
            }
        }

        private void ReadArguments()
        {
            foreach (string s in _argumentCollection)
            {
                switch (s)
                {
                    case "-?":
                    case "?":
                    case "-HELP":
                    case "HELP":
                        ShowHelp = true;
                        continue;
                }

                string switchType = s.Substring(0, 3).ToUpper(CultureInfo.CurrentCulture);
                string switchValue = s.Remove(0, 3);

                switch (switchType)
                {
                    case "-MO":   // Mode.
                        Mode = new ModeFile(switchValue);
                        break;
					case "-SC":
						SaveSsoConfig = true;
		                break;
                    
                }
            }
        }

    }
}