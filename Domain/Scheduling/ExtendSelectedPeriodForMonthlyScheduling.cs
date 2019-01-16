using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ExtendSelectedPeriodForMonthlyScheduling
	{
		public DateOnlyPeriod Execute(SchedulingWasOrdered @event, ISchedulerStateHolder schedulerStateHolder, DateOnlyPeriod selectedPeriod)
		{
			var agents = @event.Agents.ToHashSet();
			if (@event.FromWeb && selectedPeriod.StartDate.Day == 1 && selectedPeriod.EndDate.AddDays(1).Day == 1)
			{
				var firstDaysOfWeek = new HashSet<DayOfWeek>();
				foreach (
					var person in schedulerStateHolder.SchedulingResultState.LoadedAgents.Where(x => agents.Contains(x.Id.Value)))
				{
					if (!firstDaysOfWeek.Contains(person.FirstDayOfWeek))
					{
						firstDaysOfWeek.Add(person.FirstDayOfWeek);
					}
				}

				var workweekStartsAt = firstDaysOfWeek.First();
				var firstDateInPeriodLocal = DateHelper.GetFirstDateInWeek(selectedPeriod.StartDate, workweekStartsAt);
				var lastDateInPeriodLocal = DateHelper.GetLastDateInWeek(selectedPeriod.EndDate, workweekStartsAt);
				foreach (var firstDayOfWeek in firstDaysOfWeek)
				{
					if (DateHelper.GetFirstDateInWeek(selectedPeriod.StartDate, firstDayOfWeek).CompareTo(firstDateInPeriodLocal) != 1)
					{
						firstDateInPeriodLocal = DateHelper.GetFirstDateInWeek(selectedPeriod.StartDate, firstDayOfWeek);
					}
					if (DateHelper.GetLastDateInWeek(selectedPeriod.EndDate, firstDayOfWeek).CompareTo(lastDateInPeriodLocal) == 1)
					{
						lastDateInPeriodLocal = DateHelper.GetLastDateInWeek(selectedPeriod.EndDate, firstDayOfWeek);
					}
				}
				selectedPeriod = new DateOnlyPeriod(firstDateInPeriodLocal, lastDateInPeriodLocal);
			}
			return selectedPeriod;
		}
	}
}