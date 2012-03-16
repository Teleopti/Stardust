namespace Teleopti.Support.Security
{
    class Program
    {
        private static readonly ICommandLineCommand passwordEncryption = new PasswordEncryption();
        private static readonly ICommandLineCommand forecasterDateAdjustment = new ForecasterDateAdjustment();
        private static readonly ICommandLineCommand PersonFirstDayOfWeekSetter = new PersonFirstDayOfWeekSetter();
        private static readonly ICommandLineCommand LicenseStatusChecker = new LicenseStatusChecker();

        static void Main(string[] args)
        {
            var commandLineArgument = new CommandLineArgument(args);

            if (commandLineArgument.ForecasterMode || commandLineArgument.PersonUpdateMode)
            {
                if (commandLineArgument.ForecasterMode)
                    forecasterDateAdjustment.Execute(commandLineArgument);

                if (commandLineArgument.PersonUpdateMode)
                    PersonFirstDayOfWeekSetter.Execute(commandLineArgument);
            }
            else
            {
                passwordEncryption.Execute(commandLineArgument);
            }
            LicenseStatusChecker.Execute(commandLineArgument);
        }
    }
}
