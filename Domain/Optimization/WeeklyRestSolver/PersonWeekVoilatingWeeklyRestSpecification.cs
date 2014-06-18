using System;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public interface IPersonWeekVoilatingWeeklyRestSpecification
	{
		bool IsSatisfyBy(IScheduleRange currentSchedules, PersonWeek personWeek, TimeSpan weeklyRest);
	}

	public class PersonWeekVoilatingWeeklyRestSpecification : IPersonWeekVoilatingWeeklyRestSpecification
	{
		private readonly IExtractDayOffFromGivenWeek _extractDayOffFromGivenWeek;
		private readonly IVerifyWeeklyRestAroundDayOffSpecification  _verifyWeeklyRestAroundDayOffSpecification;
		private readonly IEnsureWeeklyRestRule  _ensureWeeklyRestRule;

		public PersonWeekVoilatingWeeklyRestSpecification(IExtractDayOffFromGivenWeek extractDayOffFromGivenWeek, IVerifyWeeklyRestAroundDayOffSpecification verifyWeeklyRestAroundDayOffSpecification, IEnsureWeeklyRestRule ensureWeeklyRestRule)
		{
			_extractDayOffFromGivenWeek = extractDayOffFromGivenWeek;
			_verifyWeeklyRestAroundDayOffSpecification = verifyWeeklyRestAroundDayOffSpecification;
			_ensureWeeklyRestRule = ensureWeeklyRestRule;
		}

		public bool IsSatisfyBy(IScheduleRange currentSchedules, PersonWeek personWeek, TimeSpan weeklyRest)
		{
			var scheduleDayList = currentSchedules.ScheduledDayCollection(personWeek.Week);
			var daysOffInProvidedWeek = _extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList);
			if (_verifyWeeklyRestAroundDayOffSpecification.IsSatisfy(daysOffInProvidedWeek, currentSchedules))
				if (!_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, currentSchedules, weeklyRest))
					return false;
			return true;
		}
	}
}