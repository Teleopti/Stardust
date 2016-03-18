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
		void UpdateGroupPage(AnalyticsGroupPage analyticsGroupPage);
	}
}