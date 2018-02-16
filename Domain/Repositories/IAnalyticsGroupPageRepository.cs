using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsGroupPageRepository
	{
		void AddGroupPageIfNotExisting(AnalyticsGroup analyticsGroup);
		AnalyticsGroup AddAndGetGroupPage(AnalyticsGroup analyticsGroup);
		void DeleteGroupPages(IEnumerable<Guid> groupPageIds, Guid businessUnitCode);
		IEnumerable<AnalyticsGroup> GetGroupPage(Guid groupPageCode, Guid businessUnitCode);
		AnalyticsGroup GetGroupPageByGroupCode(Guid groupCode, Guid businessUnitCode);
		void UpdateGroupPage(AnalyticsGroup analyticsGroup);
		void DeleteGroupPagesByGroupCodes(IEnumerable<Guid> groupCodes, Guid businessUnitCode);
		IEnumerable<AnalyticsGroupPage> GetBuildInGroupPageBase(Guid businessUnitCode);
	}
}