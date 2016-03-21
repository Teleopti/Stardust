using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsBridgeGroupPagePersonRepository
	{
		void DeleteAllBridgeGroupPagePerson(IEnumerable<Guid> groupPageIds);
		void DeleteBridgeGroupPagePerson(IEnumerable<Guid> personIds, Guid groupId);
		void AddBridgeGroupPagePerson(IEnumerable<Guid> personIds, Guid groupId);
		IEnumerable<Guid> GetBridgeGroupPagePerson(Guid groupId);

		
	}
}