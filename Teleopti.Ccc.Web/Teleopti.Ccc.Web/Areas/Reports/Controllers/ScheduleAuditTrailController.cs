using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Global.Models;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Reports.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailWebReport)]
	public class ScheduleAuditTrailController : ApiController
	{
		private readonly PersonsWhoChangedSchedulesViewModelProvider _personsWhoChangedSchedulesViewModelProvider;
		private readonly OrganizationSelectionProvider _organizationSelectionProvider;
		private readonly ScheduleAuditTrailReportViewModelProvider _scheduleAuditTrailReportViewModelProvider;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IUserUiCulture _uiCulture;
		private readonly IPermissionProvider _permissionProvider;


		public ScheduleAuditTrailController(PersonsWhoChangedSchedulesViewModelProvider personsWhoChangedSchedulesViewModelProvider,
			OrganizationSelectionProvider organizationSelectionProvider,
			ScheduleAuditTrailReportViewModelProvider scheduleAuditTrailReportViewModelProvider, 
			IGroupingReadOnlyRepository groupingReadOnlyRepository,
			ITeamRepository teamRepository, IUserUiCulture uiCulture, IPermissionProvider permissionProvider)
		{
			_personsWhoChangedSchedulesViewModelProvider = personsWhoChangedSchedulesViewModelProvider;
			_organizationSelectionProvider = organizationSelectionProvider;
			_scheduleAuditTrailReportViewModelProvider = scheduleAuditTrailReportViewModelProvider;
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_teamRepository = teamRepository;
			_uiCulture = uiCulture;
			_permissionProvider = permissionProvider;
		}

		[UnitOfWork, HttpGet, Route("api/Reports/PersonsWhoChangedSchedules")]
		public virtual IHttpActionResult PersonsWhoChangedSchedules()
		{
			return Ok(_personsWhoChangedSchedulesViewModelProvider.Provide());
		}

		[UnitOfWork, HttpPost, Route("api/Reports/ScheduleAuditTrailReport")]
		public virtual IHttpActionResult ScheduleAuditTrailReport([FromBody] AuditTrailSearchParams value)
		{
			return Ok(_scheduleAuditTrailReportViewModelProvider.Provide(value));
		}


		[UnitOfWork, HttpGet, Route("api/Reports/OrganizationSelectionAuditTrail")]
		public virtual object OrganizationSelectionAuditTrail()
		{
			return _organizationSelectionProvider.Provide();
		}

		[UnitOfWork, HttpPost, Route("api/Reports/OrganizationSelectionWithPermissionAuditTrail")]
		public virtual SiteViewModelWithTeams[] OrganizationSelectionWithPermissionAuditTrail(DateTime startDate, DateTime endDate)
		{
			var period = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
			var stringComparer = StringComparer.Create(_uiCulture.GetUiCulture(), false);
			var mainGroupPage =
				_groupingReadOnlyRepository.AvailableGroups(period, Group.PageMainId)
					.ToLookup(t => t.PageId);
			var ret = new List<SiteViewModelWithTeams>();
			var mainGroupPageBySites = mainGroupPage[Group.PageMainId].ToLookup(g => g.SiteId);
			foreach (var siteLookUp in mainGroupPageBySites)
			{
				var teamGroups = mainGroupPageBySites[siteLookUp.Key].Where(team => _permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailWebReport, period.StartDate, team));
				if (!teamGroups.Any())
					continue;
				var teams = _teamRepository.FindTeams(teamGroups.Select(x => x.GroupId));
				var children = teams.Select(t => new TeamViewModel
				{
					Name = t.Description.Name,
					Id = t.Id.GetValueOrDefault()
				}).OrderBy(c => c.Name, stringComparer);
				ret.Add(new SiteViewModelWithTeams
				{

					Name = teams.First().Site.Description.Name,
					Id = siteLookUp.Key.GetValueOrDefault(),
					Children = children.ToList()
				});
			}
			return ret.ToArray();
		}
	}
}