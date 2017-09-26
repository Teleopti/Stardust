using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Reports
{
	public class ScheduleAuditTrailReportViewModelProvider
	{
		private readonly IScheduleAuditTrailReport _scheduleAuditTrailReport;
		private readonly IUserTimeZone _timeZone;

		public ScheduleAuditTrailReportViewModelProvider(IScheduleAuditTrailReport scheduleAuditTrailReport, IUserTimeZone timeZone)
		{
			_scheduleAuditTrailReport = scheduleAuditTrailReport;
			_timeZone = timeZone;
		}

		public IList<ScheduleAuditingReportData> Provide(AuditTrailSearchParams searchParam)
		{
			var changeOccurredPeriod =
				new DateTimePeriod(TimeZoneHelper.ConvertToUtc(searchParam.ChangesOccurredStartDate, _timeZone.TimeZone()),
					TimeZoneHelper.ConvertToUtc(searchParam.ChangesOccurredEndDate.AddDays(1).AddTicks(-1), _timeZone.TimeZone()));

			var affectedPeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(searchParam.AffectedPeriodStartDate, _timeZone.TimeZone()),
				TimeZoneHelper.ConvertToUtc(searchParam.AffectedPeriodEndDate.AddDays(1).AddTicks(-1), _timeZone.TimeZone()));

			 return _scheduleAuditTrailReport.Report(searchParam.ChangedByPersonId, changeOccurredPeriod, affectedPeriod);
		}
	}
}
