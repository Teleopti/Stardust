using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public interface IDelayedSetup
	{
		void Apply(IPerson user, ICurrentUnitOfWork currentUnitOfWork);
	}
}