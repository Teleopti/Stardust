using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Reports
{
	public class ScheduleChangedByUserViewModelProvider
	{
		private readonly IScheduleHistoryReport _scheduleHistoryReport;

		public ScheduleChangedByUserViewModelProvider(IScheduleHistoryReport scheduleHistoryReport)
		{
			_scheduleHistoryReport = scheduleHistoryReport;
		}
		public IList<ChangedBy> Provide()
		{
			return _scheduleHistoryReport.RevisionPeople()
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
