using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsBridgeGroupPagePersonRepository : IAnalyticsBridgeGroupPagePersonRepository
	{
		public void DeleteAllBridgeGroupPagePerson(ICollection<Guid> groupPageIds, Guid businessUnitId)
		{
			throw new NotImplementedException();
		}

		public void DeleteBridgeGroupPagePerson(ICollection<Guid> personIds, Guid groupId, Guid businessUnitId)
		{
			throw new NotImplementedException();
		}

		public void AddBridgeGroupPagePerson(ICollection<Guid> personIds, Guid groupId, Guid businessUnitId)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Guid> GetBridgeGroupPagePerson(Guid groupId, Guid businessUnitId)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Guid> GetGroupPagesForPersonPeriod(int personId, Guid businessUnitId)
		{
			throw new NotImplementedException();
		}

		public void DeleteBridgeGroupPagePersonForPersonPeriod(int personId, ICollection<Guid> groupIds, Guid businessUnitId)
		{
			throw new NotImplementedException();
		}

		public void AddBridgeGroupPagePersonForPersonPeriod(int personId, ICollection<Guid> groupIds, Guid businessUnitId)
		{
			throw new NotImplementedException();
		}

		public void DeleteBridgeGroupPagePersonExcludingPersonPeriods(Guid personId, ICollection<int> personPeriodIds)
		{
			throw new NotImplementedException();
		}
	}
}