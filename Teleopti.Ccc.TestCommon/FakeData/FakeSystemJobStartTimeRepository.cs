using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSkillForecastJobStartTimeRepository : ISkillForecastJobStartTimeRepository
	{
		private readonly INow _now;
		public List<FakeStartTimeModel> EntryList = new List<FakeStartTimeModel>();
		private readonly SkillForecastSettingsReader _skillForecastSettingsReader;

		public FakeSkillForecastJobStartTimeRepository(INow now, SkillForecastSettingsReader skillForecastSettingsReader)
		{
			_now = now;
			_skillForecastSettingsReader = skillForecastSettingsReader;
		}

		public DateTime? GetLastCalculatedTime(Guid bu)
		{
			return EntryList.FirstOrDefault(x => x.BusinessUnit == bu)?.StartedAt;
		}

		public bool UpdateJobStartTime(Guid bu)
		{
			var entry = EntryList.SingleOrDefault(x => x.BusinessUnit == bu );
			if (entry != null)
			{
				if (_now.UtcDateTime() < entry.Locked)
					return true;
				entry.StartedAt = _now.UtcDateTime();
				entry.Locked = _now.UtcDateTime()
					.AddMinutes(_skillForecastSettingsReader.MaximumEstimatedExecutionTimeOfJobInMinutes);
				//return false;
			}
			else
			{
				EntryList.Add(new FakeStartTimeModel()
				{
					StartedAt = _now.UtcDateTime(),
					BusinessUnit = bu,
					Locked = _now.UtcDateTime().AddMinutes(_skillForecastSettingsReader.MaximumEstimatedExecutionTimeOfJobInMinutes)
				});
			}

			return false;
		}

		public bool IsLockTimeValid(Guid businessUnitId)
		{
			var entry = EntryList.FirstOrDefault(x => x.BusinessUnit == businessUnitId );
			if (entry.Locked.HasValue && _now.UtcDateTime() >= entry.Locked)
				return false;
			return true;
		}

		public void ResetLock(Guid businessUnitId)
		{
			var entry = EntryList.SingleOrDefault(x => x.BusinessUnit == businessUnitId);
			if(entry!=null)
			entry.Locked = null;
		}
	}


	public class FakeStartTimeModel
	{
		public DateTime StartedAt { get; set; }
		public Guid BusinessUnit { get; set; }
		public DateTime? Locked { get; set; }
	}
}