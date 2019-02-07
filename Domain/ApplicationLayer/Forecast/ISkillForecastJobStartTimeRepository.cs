﻿using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
	public interface ISkillForecastJobStartTimeRepository
	{
		DateTime? GetLastCalculatedTime(Guid businessUnitId);
		void UpdateJobStartTime(Guid businessUnitId);
		bool IsLockTimeValid(Guid businessUnitId);
		void ResetLock(Guid businessUnitId);
	}
}