using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsGroupPageRepository
	{
		void AddGroupPage(AnalyticsGroupPage analyticsGroupPage);
		void DeleteGroupPages(IEnumerable<Guid> groupPageIds);
		IEnumerable<AnalyticsGroupPage> GetGroupPage(Guid groupPageCode);
		AnalyticsGroupPage GetGroupPageByGroupCode(Guid groupCode);
		void UpdateGroupPage(AnalyticsGroupPage analyticsGroupPage);
		AnalyticsGroupPage FindGroupPageByGroupName(string groupName);
		Guid FindGroupPageCodeByResourceKey(string resourceKey);
		void DeleteGroupPagesByGroupCodes(IEnumerable<Guid> groupCodes);
	}
}