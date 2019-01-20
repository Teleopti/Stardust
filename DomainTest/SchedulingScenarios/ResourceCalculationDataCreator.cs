using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	public static class ResourceCalculationDataCreator
	{
		public static ResourceCalculationData WithData(IScenario scenario, 
											DateOnly date, 
											IEnumerable<IPersistableScheduleData> persistableScheduleData, 
											IEnumerable<ISkillDay> skillDays,
											bool considerShortbreaks,
											bool doIntraIntervalCalculation)
		{
			return WithData(scenario, date.ToDateOnlyPeriod(), persistableScheduleData, skillDays, considerShortbreaks, doIntraIntervalCalculation);
		}
		
		public static ResourceCalculationData WithData(IScenario scenario,
			DateOnlyPeriod period,
			IEnumerable<IPersistableScheduleData> persistableScheduleData,
			IEnumerable<ISkillDay> skillDays,
			bool considerShortbreaks,
			bool doIntraIntervalCalculation)
		{
			var skillDaysDic = skillDays.GroupBy(x => x.Skill).ToDictionary(k=> k.Key, v => v.AsEnumerable());

			return new ResourceCalculationData(
				ScheduleDictionaryCreator.WithData(scenario, period, persistableScheduleData),
				skillDaysDic, considerShortbreaks, doIntraIntervalCalculation);
		}
		
		public static ResourceCalculationData WithData(IScenario scenario,
			DateOnly date,
			ISkillDay skillDay)
		{
			return WithData(scenario, date.ToDateOnlyPeriod(), Enumerable.Empty<IPersistableScheduleData>(), new[] {skillDay},
				false, false);
		}
				
		public static ResourceCalculationData WithData(IScenario scenario,
			DateOnly date,
			IEnumerable<ISkillDay> skillDays)
		{
			return WithData(scenario, date.ToDateOnlyPeriod(), Enumerable.Empty<IPersistableScheduleData>(), skillDays, false, false);
		}

		public static ResourceCalculationData WithData(IShovelingCallback shovelingCallback,
									IScenario scenario,
									DateOnly date,
									IEnumerable<IPersistableScheduleData> persistableScheduleData,
									IEnumerable<ISkillDay> skillDays,
									bool considerShortbreaks,
									bool doIntraIntervalCalculation)
		{
			var ret = WithData(scenario, date.ToDateOnlyPeriod(), persistableScheduleData, skillDays, considerShortbreaks, doIntraIntervalCalculation);
			ret.SetShovelingCallback(shovelingCallback);
			return ret;
		}
	}
}