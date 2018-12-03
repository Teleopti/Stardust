using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Reports
{
	public class ScheduleAuditTrailReportViewModelProvider
	{
		private readonly IScheduleAuditTrailReport _scheduleAuditTrailReport;
		private readonly IPersonRepository _personRepository;
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;

		public ScheduleAuditTrailReportViewModelProvider(IScheduleAuditTrailReport scheduleAuditTrailReport, IPersonRepository personRepository, IPersonFinderReadOnlyRepository personFinderReadOnlyRepository)
		{
			_scheduleAuditTrailReport = scheduleAuditTrailReport;
			_personRepository = personRepository;
			_personFinderReadOnlyRepository = personFinderReadOnlyRepository;
		}

		public IList<ScheduleAuditingReportData> Provide(AuditTrailSearchParams searchParam)
		{
			var changeOccurredPeriod = new DateOnlyPeriod(new DateOnly(searchParam.ChangesOccurredStartDate),
				new DateOnly(searchParam.ChangesOccurredEndDate));
			var affectedPeriod = new DateOnlyPeriod(new DateOnly(searchParam.AffectedPeriodStartDate),
				new DateOnly(searchParam.AffectedPeriodEndDate));
			var changedByPerson = _personRepository.Get(searchParam.ChangedByPersonId);
			var scheduledAgentIds =
				_personFinderReadOnlyRepository.FindPersonIdsInTeams(affectedPeriod.EndDate, searchParam.TeamIds,
					new Dictionary<PersonFinderField, string>());
			var scheduledAgents = scheduledAgentIds
				.Select(scheduledAgentId => _personRepository.Get(scheduledAgentId))
				.ToList();
			
			return _scheduleAuditTrailReport.Report(changedByPerson, changeOccurredPeriod, affectedPeriod, searchParam.MaximumResults, scheduledAgents);
		}
	}
}
