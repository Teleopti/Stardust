using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IPeopleAccountUpdaterProvider
	{
		ITraceableRefreshService RefreshService { get; }
		IUnitOfWork UnitOfWork { get; }
		IEnumerable<KeyValuePair<IPerson, IPersonAccountCollection>> PersonAccounts(IPerson person);
	}
}
