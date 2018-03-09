using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class SkillCombinationResourceReadModelValidator
	{
		private readonly IRequestStrategySettingsReader _requestStrategySettingReader;
		private readonly INow _now;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;

		public SkillCombinationResourceReadModelValidator(IRequestStrategySettingsReader requestStrategySettingReader,
			INow now, ISkillCombinationResourceRepository skillCombinationResourceRepository)
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
}