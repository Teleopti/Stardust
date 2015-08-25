namespace Teleopti.Support.Security
{
    internal interface ICommandLineCommand
    {
        int Execute(IDatabaseArguments databaseArguments);
    }
}