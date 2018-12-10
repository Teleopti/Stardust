namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IPersonLeavingUpdater
	{
		void Execute(DateOnly leavingDate, IPerson person);
	}
}