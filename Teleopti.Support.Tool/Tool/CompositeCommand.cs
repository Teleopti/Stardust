
namespace Teleopti.Support.Tool.Tool
{
	public class CompositeCommand : ISupportCommand
	{
		private readonly ISupportCommand[] _commands;

		public CompositeCommand(params ISupportCommand[] commands)
		{
			_commands = commands;
		}

		public void Execute()
		{
			foreach (var supportCommand in _commands)
			{
				supportCommand.Execute();
			}
		}
	}
}