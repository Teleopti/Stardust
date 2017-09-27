using System.Collections.Generic;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Reports
{
	public class ScheduleAuditTrailReportViewModelProvider
	{
		private readonly IScheduleAuditTrailReport _scheduleAuditTrailReport;
		private readonly IPersonRepository _personRepository;

		public ScheduleAuditTrailReportViewModelProvider(IScheduleAuditTrailReport scheduleAuditTrailReport, IPersonRepository personRepository)
		{
			_scheduleAuditTrailReport = scheduleAuditTrailReport;
			_personRepository = personRepository;
		}

		public IList<ScheduleAuditingReportData> Provide(AuditTrailSearchParams searchParam)
		{
			var changeOccurredPeriod = new DateOnlyPeriod(new DateOnly(searchParam.ChangesOccurredStartDate), new DateOnly(searchParam.ChangesOccurredEndDate));
			var affectedPeriod = new DateOnlyPeriod(new DateOnly(searchParam.AffectedPeriodStartDate), new DateOnly(searchParam.AffectedPeriodEndDate));
			var changedByPerson = _personRepository.Get(searchParam.ChangedByPersonId);

			return _scheduleAuditTrailReport.Report(changedByPerson, changeOccurredPeriod, affectedPeriod, searchParam.MaximumResults);
		}
	}
}
