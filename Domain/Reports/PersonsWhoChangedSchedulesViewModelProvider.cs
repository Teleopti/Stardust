using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Reports
{
	public class PersonsWhoChangedSchedulesViewModelProvider
	{
		private readonly IScheduleAuditTrailReport _scheduleAuditTrailReport;

		public PersonsWhoChangedSchedulesViewModelProvider(IScheduleAuditTrailReport scheduleAuditTrailReport)
		{
			_scheduleAuditTrailReport = scheduleAuditTrailReport;
		}

		public IList<SimplestPersonInfo> Provide()
		{
			return _scheduleAuditTrailReport.GetRevisionPeople().ToList();
		}
	}
}
