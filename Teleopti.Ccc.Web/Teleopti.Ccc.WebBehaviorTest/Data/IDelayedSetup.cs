using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public interface IDelayedSetup
	{
		void Apply(IPerson user, ICurrentUnitOfWork currentUnitOfWork);
	}
}