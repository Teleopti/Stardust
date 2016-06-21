using System;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsPermissionExecutionRepository
	{
		DateTime Get(Guid personId, int businessUnitId);
		void Set(Guid personId, int businessUnitId);
	}
}