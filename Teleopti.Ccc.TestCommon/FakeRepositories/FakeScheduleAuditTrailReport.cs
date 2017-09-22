using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleAuditTrailReport : IScheduleAuditTrailReport
	{
		private readonly IList<IPerson> modifiedByList = new List<IPerson>();

		public void AddModifiedByPerson(IPerson PersonThatModified)
		{
			modifiedByList.Add(PersonThatModified);
		}
		
		public IEnumerable<IPerson> RevisionPeople()
		{
			return modifiedByList;
		}
	}
}
