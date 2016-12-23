using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsGroupPageRepository : IAnalyticsGroupPageRepository
	{
		private readonly List<AnalyticsGroup> groups = new List<AnalyticsGroup>();

		public void AddGroupPageIfNotExisting(AnalyticsGroup analyticsGroup)
		{
			if (groups.Any(x => x.BusinessUnitCode == analyticsGroup.BusinessUnitCode && x.GroupCode == analyticsGroup.GroupCode))
				return;
			groups.Add(analyticsGroup);
		}

		public void DeleteGroupPages(IEnumerable<Guid> groupPageIds, Guid businessUnitCode)
		{
			var codes = groupPageIds.ToList();
			groups.RemoveAll(x => x.BusinessUnitCode == businessUnitCode && codes.Contains(x.GroupPageCode));
		}

		public IEnumerable<AnalyticsGroup> GetGroupPage(Guid groupPageCode, Guid businessUnitCode)
		{
			return groups.Where(x => x.GroupPageCode == groupPageCode && x.BusinessUnitCode == businessUnitCode).ToList();
		}

		public AnalyticsGroup GetGroupPageByGroupCode(Guid groupCode, Guid businessUnitCode)
		{
			return groups.FirstOrDefault(x => x.GroupCode == groupCode && x.BusinessUnitCode == businessUnitCode);
		}

		public void UpdateGroupPage(AnalyticsGroup analyticsGroup)
		{
			groups.RemoveAll(x =>
					x.GroupCode == analyticsGroup.GroupCode && 
					x.GroupPageCode == analyticsGroup.GroupPageCode &&
					x.BusinessUnitCode == analyticsGroup.BusinessUnitCode);
			groups.Add(analyticsGroup);
		}

		public void DeleteGroupPagesByGroupCodes(IEnumerable<Guid> groupCodes, Guid businessUnitCode)
		{
			var codes = groupCodes.ToList();
			groups.RemoveAll(x => x.BusinessUnitCode == businessUnitCode && codes.Contains(x.GroupCode));
		}

		public IEnumerable<AnalyticsGroupPage> GetBuildInGroupPageBase(Guid businessUnitCode)
		{
			return groups.Where(x => x.GroupPageNameResourceKey != null && x.BusinessUnitCode == businessUnitCode).ToList();
		}
	}
}