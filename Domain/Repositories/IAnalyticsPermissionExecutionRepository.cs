using System;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsPermissionExecutionRepository
	{
		DateTime Get(Guid personId);
		void Set(Guid personId);
	}
}