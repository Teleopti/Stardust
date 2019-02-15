using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
	public interface ISkillForecastJobStartTimeRepository
	{
		DateTime? GetLastCalculatedTime(Guid businessUnitId);
		bool UpdateJobStartTime(Guid businessUnitId);
		bool IsLockTimeValid(Guid businessUnitId);
		void ResetLock(Guid businessUnitId, string connectionString = "");
	}
}