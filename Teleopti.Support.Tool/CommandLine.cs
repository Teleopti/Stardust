using Teleopti.Support.Code.Tool;

namespace Teleopti.Support.Tool
{
    public class CommandLine
    {
        public static void Main(string[] args)
        {
            if (args.Length.Equals(0))
                new MainForm().ShowDialog();
            else
            {
                var commandLineArgument = new CommandLineArgument(args);
                if (commandLineArgument.ShowHelp)
                    new HelpWindow(commandLineArgument.Help).ShowDialog();
            }
           
        }
    }
}