namespace Teleopti.Support.Security
{
    internal interface ICommandLineCommand
    {
        void Execute(CommandLineArgument commandLineArgument);
    }
}