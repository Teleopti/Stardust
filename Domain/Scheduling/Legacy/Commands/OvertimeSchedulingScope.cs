using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public static class OvertimeSchedulingScope
	{
		/*
		 * Temporarly hack to fix...
		 * * Need to "turn off" nightly rest rule if user has checked AllowBreakNightlyRest
		 * ...we'll fix these one-by-one later
		 */
		public static IDisposable Set(IScheduleDay scheduleDay, IOvertimePreferences overtimePreferences)
		{
			var agent = scheduleDay.Person;
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var oldWorkTimeDirective = agent.Period(date).PersonContract.Contract.WorkTimeDirective;
			if (overtimePreferences.AllowBreakNightlyRest)
			{
				agent.Period(date).PersonContract.Contract.WorkTimeDirective = 
					new WorkTimeDirective(oldWorkTimeDirective.MinTimePerWeek, oldWorkTimeDirective.MaxTimePerWeek, TimeSpan.Zero, oldWorkTimeDirective.WeeklyRest);
			}

			return new GenericDisposable(() =>
			{
				scheduleDay.Person.Period(date).PersonContract.Contract.WorkTimeDirective = oldWorkTimeDirective;
			});
		}
	}
}