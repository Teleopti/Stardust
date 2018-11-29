using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class DummyPersonLeavingUpdater : IPersonLeavingUpdater
	{
		public void Execute(DateOnly leavingDate, IPerson person)
		{
		}
	}
}