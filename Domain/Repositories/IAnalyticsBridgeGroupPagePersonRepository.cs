using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsBridgeGroupPagePersonRepository
	{
		void DeleteAllBridgeGroupPagePerson(IEnumerable<Guid> groupPageIds, Guid businessUnitId);
		void DeleteBridgeGroupPagePerson(IEnumerable<Guid> personIds, Guid groupId, Guid businessUnitId);
		void AddBridgeGroupPagePerson(IEnumerable<Guid> personIds, Guid groupId, Guid businessUnitId);
		IEnumerable<Guid> GetBridgeGroupPagePerson(Guid groupId, Guid businessUnitId);
		IEnumerable<Guid> GetGroupPagesForPersonPeriod(Guid personPeriodId, Guid businessUnitId);
		void DeleteBridgeGroupPagePersonForPersonPeriod(Guid personPeriodId, IEnumerable<Guid> groupIds, Guid businessUnitId);
		void AddBridgeGroupPagePersonForPersonPeriod(Guid personPeriodId, IEnumerable<Guid> groupIds, Guid businessUnitId);
		void DeleteBridgeGroupPagePersonExcludingPersonPeriods(Guid personId, IEnumerable<Guid> personPeriodIds, Guid businessUnitId);
	}
}