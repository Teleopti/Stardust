using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsBridgeGroupPagePersonRepository
	{
		void DeleteAllBridgeGroupPagePerson(ICollection<Guid> groupPageIds, Guid businessUnitId);
		void DeleteBridgeGroupPagePerson(ICollection<Guid> personIds, Guid groupId, Guid businessUnitId);
		void AddBridgeGroupPagePerson(ICollection<Guid> personIds, Guid groupId, Guid businessUnitId);
		IEnumerable<Guid> GetBridgeGroupPagePerson(Guid groupId, Guid businessUnitId);
		IEnumerable<Guid> GetGroupPagesForPersonPeriod(int personId, Guid businessUnitId);
		void DeleteBridgeGroupPagePersonForPersonPeriod(int personId, ICollection<Guid> groupIds, Guid businessUnitId);
		void AddBridgeGroupPagePersonForPersonPeriod(int personId, ICollection<Guid> groupIds, Guid businessUnitId);
		void DeleteBridgeGroupPagePersonExcludingPersonPeriods(Guid personCode, ICollection<int> personPeriodIds);
		void DeleteAllForPersons(Guid groupPageId, ICollection<Guid> personIds, Guid businessUnitId);
	}
}