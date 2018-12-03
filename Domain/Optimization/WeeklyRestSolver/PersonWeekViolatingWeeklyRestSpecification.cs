using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public interface IPersonWeekViolatingWeeklyRestSpecification
	{
		bool IsSatisfyBy(IScheduleRange currentSchedules, DateOnlyPeriod week, TimeSpan weeklyRest);
	}

	public class PersonWeekViolatingWeeklyRestSpecification : IPersonWeekViolatingWeeklyRestSpecification
	{
		private readonly IExtractDayOffFromGivenWeek _extractDayOffFromGivenWeek;
		private readonly IVerifyWeeklyRestAroundDayOffSpecification  _verifyWeeklyRestAroundDayOffSpecification;
		private readonly IEnsureWeeklyRestRule  _ensureWeeklyRestRule;

		public PersonWeekViolatingWeeklyRestSpecification(IExtractDayOffFromGivenWeek extractDayOffFromGivenWeek, IVerifyWeeklyRestAroundDayOffSpecification verifyWeeklyRestAroundDayOffSpecification, IEnsureWeeklyRestRule ensureWeeklyRestRule)
		{
			_extractDayOffFromGivenWeek = extractDayOffFromGivenWeek;
			_verifyWeeklyRestAroundDayOffSpecification = verifyWeeklyRestAroundDayOffSpecification;
			_ensureWeeklyRestRule = ensureWeeklyRestRule;
		}

		public bool IsSatisfyBy(IScheduleRange currentSchedules, DateOnlyPeriod week, TimeSpan weeklyRest)
		{
			var personWeek = new PersonWeek(currentSchedules.Person, week);
			var scheduleDayList = currentSchedules.ScheduledDayCollection(week);
			var daysOffInProvidedWeek = _extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList);
			if (_verifyWeeklyRestAroundDayOffSpecification.IsSatisfy(daysOffInProvidedWeek, currentSchedules))
				if (!_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, currentSchedules, weeklyRest))
					return false;
			return true;
		}
	}
}