namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface ICommandDispatcher
	{
		void Invoke(object command);
	}
}