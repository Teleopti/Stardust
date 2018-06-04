using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

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
				foreach (var dateOnly in selectedPeriod.DayCollection())
				{
					virtualSchedulePeriods.Add(person.VirtualSchedulePeriod(dateOnly));
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
	public enum RestrictionNotAbleToBeScheduledReason
	{
		TooManyDaysOff,
		TooMuchWorkTimeInPeriod,
		TooLittleWorkTimeInPeriod,
		NightlyRestMightBeBroken,
		ConflictingRestrictions,
		NoIssue
	}
}