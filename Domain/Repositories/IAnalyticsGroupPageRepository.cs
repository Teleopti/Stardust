using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsGroupPageRepository
	{
		void AddGroupPage(AnalyticsGroup analyticsGroup);
		void DeleteGroupPages(IEnumerable<Guid> groupPageIds);
		IEnumerable<AnalyticsGroup> GetGroupPage(Guid groupPageCode);
		AnalyticsGroup GetGroupPageByGroupCode(Guid groupCode);
		void UpdateGroupPage(AnalyticsGroup analyticsGroup);
		void DeleteGroupPagesByGroupCodes(IEnumerable<Guid> groupCodes);
		IEnumerable<AnalyticsGroupPage> GetBuildInGroupPageBase();
	}
}