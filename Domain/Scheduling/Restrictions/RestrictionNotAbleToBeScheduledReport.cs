using System.Collections.Generic;
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
		public IEnumerable<RestrictionsNotAbleToBeScheduledResult> Create(DateOnly date, IEnumerable<IPerson> persons)
		{
			var report = new List<RestrictionsNotAbleToBeScheduledResult>();
			foreach (var person in persons)
			{
				RestrictionsNotAbleToBeScheduledResult failReason = _restrictionsAbleToBeScheduled.Execute(person.VirtualSchedulePeriod(date));
				if(failReason != null)
					report.Add(failReason);
			}

			return report;
		}
	}

	public class RestrictionsNotAbleToBeScheduledResult
	{
		public IPerson Agent { get; set; }
		public RestrictionNotAbleToBeScheduledReason Reason { get; set; }
		public DateOnlyPeriod Period { get; set; }
	}

	public enum RestrictionNotAbleToBeScheduledReason
	{
		//TooManyDaysOff,
		TooMuchWorkTimeInPeriod,
		TooLittleWorkTimeInPeriod,
		NightlyRestMightBeBroken
	}
}