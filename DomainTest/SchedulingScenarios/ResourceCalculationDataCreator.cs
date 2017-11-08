using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

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
			return WithData(scenario, period, persistableScheduleData, skillDays, Enumerable.Empty<BpoResource>(),
				considerShortbreaks, doIntraIntervalCalculation);
		}
		
		public static ResourceCalculationData WithData(IScenario scenario,
			DateOnlyPeriod period,
			IEnumerable<IPersistableScheduleData> persistableScheduleData,
			IEnumerable<ISkillDay> skillDays,
			IEnumerable<BpoResource> bpoResources,
			bool considerShortbreaks,
			bool doIntraIntervalCalculation)
		{
			var skillDaysDic = skillDays.GroupBy(x => x.Skill).ToDictionary(k=> k.Key, v => v.AsEnumerable());

			return new ResourceCalculationData(
				ScheduleDictionaryCreator.WithData(scenario, period, persistableScheduleData),
				skillDaysDic.Keys, skillDaysDic, bpoResources, considerShortbreaks, doIntraIntervalCalculation);
		}
		
		public static ResourceCalculationData WithData(IScenario scenario,
			DateOnly date,
			ISkillDay skillDay,
			BpoResource bpoResource)
		{
			return WithData(scenario, date.ToDateOnlyPeriod(), Enumerable.Empty<IPersistableScheduleData>(), new[] {skillDay},
				new[] {bpoResource}, false, false);
		}
		
		public static ResourceCalculationData WithData(IScenario scenario,
			DateOnly date,
			IEnumerable<ISkillDay> skillDays,
			BpoResource bpoResource)
		{
			return WithData(scenario, date.ToDateOnlyPeriod(), Enumerable.Empty<IPersistableScheduleData>(), skillDays,
				new[] {bpoResource}, false, false);
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