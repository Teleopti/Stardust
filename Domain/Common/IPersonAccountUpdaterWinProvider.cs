using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
	/// <summary>
	/// External depencency provider for IPeopleAccountUpdater in win
	/// </summary>
	public interface IPeopleAccountUpdaterWinProvider
	{
		ITraceableRefreshService GetRefreshService();
		IUnitOfWork GetUnitOfWork { get; }
		IPersonAccountCollection GetPersonAccounts(IPerson person);
	}
}
