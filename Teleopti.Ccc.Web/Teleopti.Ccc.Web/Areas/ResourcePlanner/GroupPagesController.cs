using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;


namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class GroupPagesController : ApiController
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly IPermissionProvider _permissionProvider;

		public GroupPagesController(IGroupingReadOnlyRepository groupingReadOnlyRepository, IPermissionProvider permissionProvider)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_permissionProvider = permissionProvider;
		}

		[Route("api/ResourcePlanner/Filter"), HttpGet, UnitOfWork]
		public virtual IEnumerable<FilterGroup> GetFilters()
		{
			var pages = _groupingReadOnlyRepository.AvailableGroupPages();
			var groups =
				_groupingReadOnlyRepository.AvailableGroups(DateOnly.Today)
					.Where(
						g =>
							_permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules,
								DateOnly.Today, g))
					.Select(g => new {g.GroupId, g.PageId, g.GroupName})
					.Distinct()
					.ToLookup(k => k.PageId);

			return pages.Select(
					p =>
						new FilterGroup
						{
							Id = p.PageId,
							Name = translatePageName(p.PageName),
							Items = groups[p.PageId].Select(g => new FilterItem { Id = g.GroupId, Name = g.GroupName }).ToList()
						}).Where(f => f.Items.Any());
		}

		private string translatePageName(string pageName)
		{
			return pageName.StartsWith("xx", StringComparison.CurrentCultureIgnoreCase)
				? UserTexts.Resources.ResourceManager.GetString(pageName.Substring(2))
				: pageName;
		}
	}
}