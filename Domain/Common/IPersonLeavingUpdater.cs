using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class DummyPersonLeavingUpdater : IPersonLeavingUpdater
	{
		public void Execute(DateOnly leavingDate, IPerson person)
		{
		}
	}
}