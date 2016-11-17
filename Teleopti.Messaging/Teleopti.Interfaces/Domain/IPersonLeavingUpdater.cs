namespace Teleopti.Interfaces.Domain
{
	public interface IPersonLeavingUpdater
	{
		void Execute(DateOnly leavingDate, IPerson person);
	}
}