namespace Teleopti.Support.Security
{
    internal interface ICommandLineCommand
    {
        int Execute(CommandLineArgument commandLineArgument);
    }
}