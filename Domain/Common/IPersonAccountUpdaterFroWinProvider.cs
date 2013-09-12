using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IPeopleAccountUpdaterInteraction
	{
		ITraceableRefreshService RefreshService { get; }
		IUnitOfWork UnitOfWork { get; }
		IPersonAccountCollection PersonAccounts(IPerson person);
	}
}
