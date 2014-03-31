using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsNoWorkShiftfFinder
	{
		bool Find(IScheduleDay scheduleDay, ISchedulingOptions schedulingOptions);
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

		public bool Find(IScheduleDay scheduleDay, ISchedulingOptions schedulingOptions)
		{
			if (scheduleDay.SignificantPartForDisplay().Equals(SchedulePartView.MainShift)) return false;
			var person = scheduleDay.Person;
			var dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var personPeriod = person.Period(dateOnly);
			if (personPeriod == null) return true;
			var ruleSetBag = personPeriod.RuleSetBag;
			if (ruleSetBag == null) return true;

			_restrictionExtractor.Extract(scheduleDay);
			var effectiveRestriction = _restrictionExtractor.CombinedRestriction(schedulingOptions);

			var minMaxWorkTime = ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, dateOnly, effectiveRestriction);

			if (minMaxWorkTime == null && effectiveRestriction.IsRestriction && effectiveRestriction.DayOffTemplate == null && effectiveRestriction.Absence == null)
			{
				return true;
			}

			return false;
		}
	}
}
