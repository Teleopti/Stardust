using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer
{
	public class LoadSkillDaysWithPeriodFlexibilityToggleOn : ILoadSkillDaysWithPeriodFlexibility
	{
		private readonly SkillDayLoadHelper _skillDayLoadHelper;

		public LoadSkillDaysWithPeriodFlexibilityToggleOn(SkillDayLoadHelper skillDayLoadHelper)
		{
			_skillDayLoadHelper = skillDayLoadHelper;
		}

		public IDictionary<ISkill, IEnumerable<ISkillDay>> Load(DateOnlyPeriod period,IEnumerable<ISkill> skills, IScenario scenario)
		{
			return _skillDayLoadHelper.LoadSkillDaysWithFlexablePeriod(period, skills, scenario);
		}
	}

	public class LoadSkillDaysWithPeriodFlexibilityToggleOff : ILoadSkillDaysWithPeriodFlexibility
	{
		private readonly SkillDayLoadHelper _skillDayLoadHelper;

		public LoadSkillDaysWithPeriodFlexibilityToggleOff(SkillDayLoadHelper skillDayLoadHelper)
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