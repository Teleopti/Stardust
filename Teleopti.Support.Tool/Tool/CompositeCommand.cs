namespace Teleopti.Support.Code.Tool
{
	public class CompositeCommand : ISupportCommand
	{
		private readonly ISupportCommand[] _commands;

		public CompositeCommand(params ISupportCommand[] commands)
		{
			_commands = commands;
		}

		public void Execute(ModeFile modeFile)
		{
			foreach (var supportCommand in _commands)
			{
				supportCommand.Execute(modeFile);
			}
		}
	}
}