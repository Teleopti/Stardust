namespace Teleopti.Support.Security
{
    class Program
    {
        private static readonly ICommandLineCommand passwordEncryption = new PasswordEncryption();
        private static readonly ICommandLineCommand forecasterDateAdjustment = new ForecasterDateAdjustment();
        private static readonly ICommandLineCommand PersonFirstDayOfWeekSetter = new PersonFirstDayOfWeekSetter();

        static void Main(string[] args)
        {
            var commandLineArgument = new CommandLineArgument(args);

            if (!commandLineArgument.ForecasterMode &&
                !commandLineArgument.PersonUpdateMode &&
                !commandLineArgument.PasswordEncryptionMode)
            {
                forecasterDateAdjustment.Execute(commandLineArgument);
                passwordEncryption.Execute(commandLineArgument);
                PersonFirstDayOfWeekSetter.Execute(commandLineArgument);
                return;
            }

            if (commandLineArgument.ForecasterMode)
                forecasterDateAdjustment.Execute(commandLineArgument);

            if (commandLineArgument.PersonUpdateMode)
                PersonFirstDayOfWeekSetter.Execute(commandLineArgument);

            if (commandLineArgument.PasswordEncryptionMode)
                passwordEncryption.Execute(commandLineArgument);


        }
    }
}
