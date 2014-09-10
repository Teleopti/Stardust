﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators
{
	public class SchedulerValidator : ISikuliValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public SchedulerValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public SikuliValidationResult Validate()
		{
			var result = new SikuliValidationResult(true);
			var scheduledHours = ValidatorHelper.GetDailyScheduledHoursForFullPeriod(_schedulerState, _totalSkill);
			var checkResult = checkScheduledHoursPatternForScheduler(scheduledHours);
			result.Details.AppendLine("Details:");
			if (checkResult)
				result.Details.AppendLine("Scheduled hours pattern : OK");
			else
			{
				result.Details.AppendLine("Scheduled hours pattern : Fail");
				result.Result = false;
			}
			return result;
		}

		private static bool checkScheduledHoursPatternForScheduler(IEnumerable<double?> dailyValues)
		{
			const int groupSize = 7;
			var groupedDailyValues = split(dailyValues.ToList(), groupSize);
			foreach (var group in groupedDailyValues)
			{
				for (int i = 0; i < groupSize; i++)
				{
					if (!group[i].HasValue)
						return false;
					if (i <= 4)
					{
						if (!group[i].Value.Equals(210d))
							return false;
					}
					else
					{
						if (!group[i].Value.Equals(0))
							return false;
					}
				}
			}
			return true;
		}

		private static IEnumerable<List<double?>> split(IEnumerable<double?> source, int groupSize)
		{
			return source
				.Select((x, i) => new { Index = i, Value = x })
				.GroupBy(x => x.Index / groupSize)
				.Select(x => x.Select(v => v.Value).ToList())
				.ToList();
		}
	}
}
