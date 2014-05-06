using System.Globalization;

namespace Teleopti.Support.Code.Tool
{
    public interface ICommandLineArgument
    {
		ModeFile Mode { get; }
        string Help { get; }
        bool ShowHelp { get; }
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