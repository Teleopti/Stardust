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
		private readonly IStardustJobFeedback _stardustJobFeedback;

		public SkillCombinationResourceReadModelValidator(IRequestStrategySettingsReader requestStrategySettingReader,
			INow now, ISkillCombinationResourceRepository skillCombinationResourceRepository,
			IStardustJobFeedback stardustJobFeedback)
		{
			_requestStrategySettingReader = requestStrategySettingReader;
			_now = now;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_stardustJobFeedback = stardustJobFeedback;
		}

		public bool Validate()
		{
			var bulkExecutionSetting = _requestStrategySettingReader.GetIntSetting("UpdateResourceReadModelIntervalMinutes", 60);
			var buStartTime = _skillCombinationResourceRepository.GetLastCalculatedTime();
			_stardustJobFeedback.SendProgress($"Fetched latest time of inserted readmodel: {buStartTime}");
			_stardustJobFeedback.SendProgress($"Utc now is: {_now.UtcDateTime()}");
			_stardustJobFeedback.SendProgress($"bulkExecutionSetting is: {bulkExecutionSetting}");
			return _now.UtcDateTime() <= buStartTime.AddMinutes(bulkExecutionSetting*2);
		}
	}
}