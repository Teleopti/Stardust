using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	public static class ResourceCalculationDataCreator
	{
		public static IResourceCalculationData WithData(IScenario scenario, 
											DateOnly date, 
											IEnumerable<IPersistableScheduleData> persistableScheduleData, 
											IEnumerable<ISkillDay> skillDays,
											bool considerShortbreaks,
											bool doIntraIntervalCalculation)
		{
			var skillDaysDic =  new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			foreach (var skillDay in skillDays)
			{
				skillDaysDic[skillDay.Skill] = new List<ISkillDay> { skillDay };
			}

			return new ResourceCalculationData(
				ScheduleDictionaryCreator.WithData(scenario, new DateOnlyPeriod(date, date), persistableScheduleData),
				skillDays.Select(x => x.Skill).Distinct(), skillDaysDic, considerShortbreaks, doIntraIntervalCalculation);
		}
	}
}