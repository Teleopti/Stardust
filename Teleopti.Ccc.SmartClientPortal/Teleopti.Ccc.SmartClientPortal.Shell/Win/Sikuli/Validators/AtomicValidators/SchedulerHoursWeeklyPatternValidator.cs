using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.AtomicValidators
{
	internal class SchedulerHoursWeeklyPatternValidator : IAtomicValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public SchedulerHoursWeeklyPatternValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public string Description 
		{ 
			get{ return "The scheduled hours must follow the pattern: 210 hours M-F, 0 hours Sa-Su.";} 
		}

		public SikuliValidationResult Validate(ITimeZoneGuard timeZoneGuard)
		{
			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var scheduledHours = ValidatorHelperMethods.GetDailyScheduledHoursForFullPeriod(_schedulerState, _totalSkill, timeZoneGuard);
			var checkResult = checkScheduledHoursPatternForScheduler(scheduledHours);
			if (!checkResult)
				result.Result = SikuliValidationResult.ResultValue.Fail;
			result.Details.AppendLine(string.Format("Scheduled hours pattern : {0}", result.Result));
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
