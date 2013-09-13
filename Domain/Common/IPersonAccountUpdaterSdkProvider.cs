using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IPeopleAccountUpdaterSdkProvider
	{
		ITraceableRefreshService GetRefreshService();
		IUnitOfWork GetUnitOfWork { get; }
		IPersonAccountCollection GetPersonAccounts(IPerson person);
	}
}
