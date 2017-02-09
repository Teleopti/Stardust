using System;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class SkillCombinationResourceReadModelValidator : ISkillCombinationResourceReadModelValidator
	{
		private readonly IRequestStrategySettingsReader _requestStrategySettingReader;
		private readonly INow _now;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		public SkillCombinationResourceReadModelValidator(IRequestStrategySettingsReader requestStrategySettingReader,  INow now, ISkillCombinationResourceRepository skillCombinationResourceRepository)
		{
			_requestStrategySettingReader = requestStrategySettingReader;
			_now = now;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
		}

		public bool Validate()
		{
			var bulkExecutionSetting = _requestStrategySettingReader.GetIntSetting("UpdateResourceReadModelIntervalMinutes", 60);
			var buStartTime = _skillCombinationResourceRepository.GetLastCalculatedTime();
			return _now.UtcDateTime() <= buStartTime.AddMinutes(bulkExecutionSetting*2);
		}
	}

	public class ScheduleForecastSkillReadModelValidator : IScheduleForecastSkillReadModelValidator
	{
		private readonly IRequestStrategySettingsReader _requestStrategySettingReader;
		private readonly INow _now;
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
		public ScheduleForecastSkillReadModelValidator(IRequestStrategySettingsReader requestStrategySettingReader,  INow now, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository)
		{
			_requestStrategySettingReader = requestStrategySettingReader;
			_now = now;
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
		}

		public bool Validate(Guid buId)
		{
			var bulkExecutionSetting = _requestStrategySettingReader.GetIntSetting("UpdateResourceReadModelIntervalMinutes", 60);
			var buStartTime = _scheduleForecastSkillReadModelRepository.GetLastCalculatedTime();
			if (buStartTime.ContainsKey(buId))
			{
				if (_now.UtcDateTime() > buStartTime[buId].AddMinutes(bulkExecutionSetting * 2))
					return false;
			}
			return true;
		}
	}

	public class ScheduleForecastSkillReadModelValidator42046ToggleDisabled : IScheduleForecastSkillReadModelValidator
	{
		public bool Validate(Guid buId)
		{
			return true;
		}
	}

	public interface IScheduleForecastSkillReadModelValidator
	{
		bool Validate(Guid buId);
	}
}