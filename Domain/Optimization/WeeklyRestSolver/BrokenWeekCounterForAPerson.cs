using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public interface IBrokenWeekCounterForAPerson
	{
		int CountBrokenWeek(IEnumerable<IScheduleDay> selectedPeriodScheduleDays, IScheduleRange personScheduleRange);
	}

	public class BrokenWeekCounterForAPerson : IBrokenWeekCounterForAPerson
	{
		private readonly IWeeksFromScheduleDaysExtractor  _weeksFromScheduleDaysExtractor;
		private readonly IEnsureWeeklyRestRule  _ensureWeeklyRestRule;
		private readonly IContractWeeklyRestForPersonWeek _contractWeeklyRestForPersonWeek;

		public BrokenWeekCounterForAPerson(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor, IEnsureWeeklyRestRule ensureWeeklyRestRule, IContractWeeklyRestForPersonWeek contractWeeklyRestForPersonWeek)
		{
			_weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
			_ensureWeeklyRestRule = ensureWeeklyRestRule;
			_contractWeeklyRestForPersonWeek = contractWeeklyRestForPersonWeek;
		}

		public int CountBrokenWeek(IEnumerable<IScheduleDay> selectedPeriodScheduleDays, IScheduleRange personScheduleRange)
		{
			var brokenWeekCount = 0;
			var selctedPersonWeeksWithNeighbouringWeeks =
				_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(selectedPeriodScheduleDays,
					true).ToList();
			foreach (var personWeek in selctedPersonWeeksWithNeighbouringWeeks)
			{
				var weeklyRest =  _contractWeeklyRestForPersonWeek.GetWeeklyRestFromContract(personWeek);
				if (!_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personScheduleRange, weeklyRest))
				{
					brokenWeekCount++;
				}
			}
			
			return brokenWeekCount;
		}
	}
}