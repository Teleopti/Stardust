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
		public IEnumerable<RestrictionsAbleToBeScheduledResult> Create(DateOnly date, IEnumerable<IPerson> persons)
		{
			var report = new List<RestrictionsAbleToBeScheduledResult>();
			foreach (var person in persons)
			{
				var success = _restrictionsAbleToBeScheduled.Execute(person.VirtualSchedulePeriod(date));
				if(!success)
					report.Add(new RestrictionsAbleToBeScheduledResult{Agent = person});
			}

			return report;
		}
	}

	public class RestrictionsAbleToBeScheduledResult
	{
		public IPerson Agent { get; set; }
	}
}