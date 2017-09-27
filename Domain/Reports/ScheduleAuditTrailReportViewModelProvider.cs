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
		private readonly IPersonRepository _personRepository;

		public ScheduleAuditTrailReportViewModelProvider(IScheduleAuditTrailReport scheduleAuditTrailReport, IUserTimeZone timeZone, IPersonRepository personRepository)
		{
			_scheduleAuditTrailReport = scheduleAuditTrailReport;
			_timeZone = timeZone;
			_personRepository = personRepository;
		}

		public IList<ScheduleAuditingReportData> Provide(AuditTrailSearchParams searchParam)
		{
			var changeOccurredPeriod = new DateOnlyPeriod(new DateOnly(searchParam.ChangesOccurredStartDate), new DateOnly(searchParam.ChangesOccurredEndDate));
			var affectedPeriod = new DateOnlyPeriod(new DateOnly(searchParam.AffectedPeriodStartDate), new DateOnly(searchParam.AffectedPeriodEndDate));
			var changedByPerson = _personRepository.Get(searchParam.ChangedByPersonId);

			return _scheduleAuditTrailReport.Report(changedByPerson, changeOccurredPeriod, affectedPeriod);
		}
	}
}
