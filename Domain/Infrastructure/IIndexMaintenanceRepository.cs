using System;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public interface IIndexMaintenanceRepository
	{
		void PerformIndexMaintenance(DatabaseEnum database);
		void SetTimespanBetweenRetries(TimeSpan span);
	}
}