using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class RestrictionNotAbleToBeScheduledReport
	{
		private readonly RestrictionsAbleToBeScheduled _restrictionsAbleToBeScheduled;

		public RestrictionNotAbleToBeScheduledReport(RestrictionsAbleToBeScheduled restrictionsAbleToBeScheduled)
		{
			_restrictionsAbleToBeScheduled = restrictionsAbleToBeScheduled;
		}
		public IEnumerable<RestrictionsNotAbleToBeScheduledResult> Create(DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> persons, ISchedulingProgress backgroundWorker)
		{
			var report = new List<RestrictionsNotAbleToBeScheduledResult>();
			
			int counter = 0;
			var virtualSchedulePeriods = new HashSet<IVirtualSchedulePeriod>();
			foreach (var person in persons)
			{
				var firstDate = selectedPeriod.StartDate;
				while (firstDate < selectedPeriod.EndDate)
				{
					var schedulePeriod = person.VirtualSchedulePeriod(firstDate);
					if (!schedulePeriod.IsValid)
					{
						firstDate = firstDate.AddDays(1);
						continue;
					}

					virtualSchedulePeriods.Add(schedulePeriod);
					firstDate = schedulePeriod.DateOnlyPeriod.EndDate.AddDays(1);
				}
			}

			var totalCount = virtualSchedulePeriods.Count;
			foreach (var virtualSchedulePeriod in virtualSchedulePeriods)
			{
				RestrictionsNotAbleToBeScheduledResult failReason = _restrictionsAbleToBeScheduled.Execute(virtualSchedulePeriod);
				if (failReason != null)
					report.Add(failReason);

				counter++;
				if (counter % 10 == 0)
				{
					backgroundWorker.ReportProgress((int)((counter / (double)totalCount) * 100), "XXAgentsAnalyzed");
				}
			}

			return report;
		}
	}

	public class RestrictionsNotAbleToBeScheduledResult
	{
		public IPerson Agent { get; set; }
		public RestrictionNotAbleToBeScheduledReason Reason { get; set; }
		public DateOnlyPeriod Period { get; set; }
		public IScheduleMatrixPro Matrix { get; set; }
	}

	//  Used for translation auto search, DO NOT REMOVE!	
	//	UserTexts.Resources.RestrictionNotAbleToBeScheduledReasonTooManyDaysOff
	//	UserTexts.Resources.RestrictionNotAbleToBeScheduledReasonTooMuchWorkTimeInPeriod
	//	UserTexts.Resources.RestrictionNotAbleToBeScheduledReasonTooLittleWorkTimeInPeriod
	//	UserTexts.Resources.RestrictionNotAbleToBeScheduledReasonNightlyRestMightBeBroken
	//	UserTexts.Resources.RestrictionNotAbleToBeScheduledReasonConflictingRestrictions
	//	UserTexts.Resources.RestrictionNotAbleToBeScheduledReasonNoIssue
	//	UserTexts.Resources.RestrictionNotAbleToBeScheduledReasonNoRestrictions
	public enum RestrictionNotAbleToBeScheduledReason
	{
		TooManyDaysOff,
		TooMuchWorkTimeInPeriod,
		TooLittleWorkTimeInPeriod,
		NightlyRestMightBeBroken,
		ConflictingRestrictions,
		NoIssue,
		NoRestrictions
	}
}