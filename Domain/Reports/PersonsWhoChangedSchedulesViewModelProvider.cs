using System;
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
		public IList<ChangedBy> Provide()
		{
			return _scheduleAuditTrailReport.RevisionPeople()
				.Select(x => new ChangedBy(){Id = x.Id.Value, Name = x.Name.ToString()})
				.OrderBy(y => y.Name)
				.ToList();
		}
	}
	public class ChangedBy
	{
		public Guid Id { get; set; }

		public string Name { get; set; }
	}
}
