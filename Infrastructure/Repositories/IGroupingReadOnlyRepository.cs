using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IGroupingReadOnlyRepository
	{
		IEnumerable<ReadOnlyGroupPage> AvailableGroupPages();
		IEnumerable<ReadOnlyGroupDetail> AvailableGroups(ReadOnlyGroupPage groupPage,DateOnly queryDate);
		IEnumerable<ReadOnlyGroupDetail> DetailsForGroup(Guid groupId, DateOnly queryDate);
	    void UpdateGroupingReadModel(ICollection<Guid> inputIds);
        void UpdateGroupingReadModelGroupPage(ICollection<Guid> inputIds);
        void UpdateGroupingReadModelData(ICollection<Guid> inputIds);
	}
}