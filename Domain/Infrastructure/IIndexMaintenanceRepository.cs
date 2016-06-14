namespace Teleopti.Ccc.Domain.Infrastructure
{
	public interface IIndexMaintenanceRepository
	{
		void PerformIndexMaintenance(string database);
	}
}