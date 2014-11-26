﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning.Scheduling
{
	/// <summary>
	/// Checks whether the week can be skipped from workshift min max calculator
	/// </summary>
	/// <remarks>
	///	The week should be skipped, if ...
	/// 1. The schedule period changes within the week and at least one of them is invalid
	/// 2. pr the MaxTimePerWeek value changes within the week
	/// Max time can be changed through the persons contract. The contract is part of the
	/// person period so it can be changed when the person period changes within the week.
	/// This can happen two ways: the current person period either starts on the week or ends on the week.
	/// In this test the person period starts within the week.
	/// </remarks>
	public class WorkShiftMinMaxCalculatorSkipWeekCheck
	{

		public bool SkipWeekCheck(IScheduleMatrixPro matrix, DateOnly dateToCheck)
		{
			var person = matrix.Person;
			var weekPeriod = DateHelper.GetWeekPeriod(dateToCheck, person.FirstDayOfWeek);
			var personPeriod = person.Period(dateToCheck);

			if (weekPeriod.Contains(personPeriod.StartDate.AddDays(-1)))
			{
				var contract = personPeriod.PersonContract.Contract;
				IPersonPeriod previousPersonPeriod = person.PreviousPeriod(personPeriod);
				if (previousPersonPeriod != null)
				{
					if (contract.WorkTimeDirective.MaxTimePerWeek != previousPersonPeriod.PersonContract.Contract.WorkTimeDirective.MaxTimePerWeek)
						return true;
				}
			}

			IPersonPeriod nextPeriod = person.NextPeriod(personPeriod);
			if (nextPeriod != null && weekPeriod.Contains(nextPeriod.StartDate))
			{
				var contract = personPeriod.PersonContract.Contract;
				if (contract.WorkTimeDirective.MaxTimePerWeek != nextPeriod.PersonContract.Contract.WorkTimeDirective.MaxTimePerWeek)
					return true;
			}

			var schedulePeriod = matrix.SchedulePeriod;

			if (weekPeriod.Contains(schedulePeriod.DateOnlyPeriod.StartDate.AddDays(-1)))
			{
				IVirtualSchedulePeriod previousSchedulePeriod =
					person.VirtualSchedulePeriod(schedulePeriod.DateOnlyPeriod.StartDate.AddDays(-1));
				if (previousSchedulePeriod == null || !previousSchedulePeriod.IsValid)
					return true;

			}

			if (weekPeriod.Contains(schedulePeriod.DateOnlyPeriod.EndDate.AddDays(+1)))
			{
				IVirtualSchedulePeriod nextSchedulePeriod =
					person.VirtualSchedulePeriod(schedulePeriod.DateOnlyPeriod.EndDate.AddDays(+1));
				if (nextSchedulePeriod == null || !nextSchedulePeriod.IsValid)
					return true;

			}


			return false;
		}

	}
}
