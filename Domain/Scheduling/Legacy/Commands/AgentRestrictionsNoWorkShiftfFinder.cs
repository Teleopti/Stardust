using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IAgentRestrictionsNoWorkShiftfFinder
	{
		bool Find(IScheduleDay scheduleDay, SchedulingOptions schedulingOptions);
	}

	public class AgentRestrictionsNoWorkShiftfFinder : IAgentRestrictionsNoWorkShiftfFinder
	{
		private readonly IRestrictionExtractor _restrictionExtractor;
		private readonly IWorkShiftWorkTime _workShiftWorkTime;

		public AgentRestrictionsNoWorkShiftfFinder(IRestrictionExtractor restrictionExtractor, IWorkShiftWorkTime workShiftWorkTime)
		{
			_restrictionExtractor = restrictionExtractor;
			_workShiftWorkTime = workShiftWorkTime;
		}

		public bool Find(IScheduleDay scheduleDay, SchedulingOptions schedulingOptions)
		{
			if (scheduleDay.SignificantPartForDisplay().Equals(SchedulePartView.MainShift)) return false;
			var person = scheduleDay.Person;
			var dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var personPeriod = person.Period(dateOnly);
			var ruleSetBag = personPeriod?.RuleSetBag;
			if (ruleSetBag == null) return true;

			var result = _restrictionExtractor.Extract(scheduleDay);
			var effectiveRestriction = result.CombinedRestriction(schedulingOptions);
			if (effectiveRestriction == null)
				return true;

			var minMaxWorkTime = ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, dateOnly, effectiveRestriction);

			if (minMaxWorkTime == null && effectiveRestriction.IsRestriction && effectiveRestriction.DayOffTemplate == null && effectiveRestriction.Absence == null)
			{
				return true;
			}

			return false;
		}
	}
}
