using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleHistoryReport : IScheduleHistoryReport
	{
		private readonly IList<IPerson> modifiedByList = new List<IPerson>();

		public void AddModifiedByPerson(IPerson PersonThatModified)
		{
			modifiedByList.Add(PersonThatModified);
		}

		public IEnumerable<ScheduleAuditingReportData> Report(IPerson modifiedBy, DateOnlyPeriod changedPeriod, DateOnlyPeriod scheduledPeriod, IEnumerable<IPerson> agents,
			int maximumRows)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ScheduleAuditingReportData> Report(DateOnlyPeriod changedPeriod, DateOnlyPeriod scheduledPeriod, IEnumerable<IPerson> agents, int maximumRows)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPerson> RevisionPeople()
		{
			return modifiedByList;
		}

		public bool LimitReached { get; }
	}
}
