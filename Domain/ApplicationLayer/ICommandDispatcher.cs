namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface ICommandDispatcher
	{
		void Execute(object command);
	}
}