using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer
{
	public class LoadSkillDaysWithPeriodFlexibilityToggleOn : ILoadSkillDaysWithPeriodFlexibility
	{
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;

		public LoadSkillDaysWithPeriodFlexibilityToggleOn(ISkillDayLoadHelper skillDayLoadHelper)
		{
			_skillDayLoadHelper = skillDayLoadHelper;
		}

		public IDictionary<ISkill, IEnumerable<ISkillDay>> Load(DateOnlyPeriod period,IEnumerable<ISkill> skills, IScenario scenario)
		{
			return _skillDayLoadHelper.LoadSchedulerSkillDays2(period, skills, scenario);
		}
	}

	public class LoadSkillDaysWithPeriodFlexibilityToggleOff : ILoadSkillDaysWithPeriodFlexibility
	{
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;

		public LoadSkillDaysWithPeriodFlexibilityToggleOff(ISkillDayLoadHelper skillDayLoadHelper)
		{
			_skillDayLoadHelper = skillDayLoadHelper;
		}

		public IDictionary<ISkill, IEnumerable<ISkillDay>> Load(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario)
		{
			return _skillDayLoadHelper.LoadSchedulerSkillDays(period, skills, scenario);
		}
	}

	public interface ILoadSkillDaysWithPeriodFlexibility
	{
		IDictionary<ISkill, IEnumerable<ISkillDay>> Load(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario);
	}
}