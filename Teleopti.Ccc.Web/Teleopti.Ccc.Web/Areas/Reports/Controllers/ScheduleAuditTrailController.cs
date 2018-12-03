using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Global.Models;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.Reports.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailWebReport)]
	public class ScheduleAuditTrailController : ApiController
	{
		private readonly PersonsWhoChangedSchedulesViewModelProvider _personsWhoChangedSchedulesViewModelProvider;
		private readonly ScheduleAuditTrailReportViewModelProvider _scheduleAuditTrailReportViewModelProvider;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IUserUiCulture _uiCulture;
		private readonly IPermissionProvider _permissionProvider;


		public ScheduleAuditTrailController(
			PersonsWhoChangedSchedulesViewModelProvider personsWhoChangedSchedulesViewModelProvider,
			ScheduleAuditTrailReportViewModelProvider scheduleAuditTrailReportViewModelProvider,
			IGroupingReadOnlyRepository groupingReadOnlyRepository,
			ITeamRepository teamRepository, IUserUiCulture uiCulture, IPermissionProvider permissionProvider)
		{
			_personsWhoChangedSchedulesViewModelProvider = personsWhoChangedSchedulesViewModelProvider;
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

		[UnitOfWork, HttpPost, Route("api/Reports/OrganizationSelectionAuditTrail")]
		public virtual SiteViewModelWithTeams[] OrganizationSelectionAuditTrail([FromBody] ValidPeriod validRange)
		{
			var period = new DateOnlyPeriod(new DateOnly(validRange.StartDate), new DateOnly(validRange.EndDate));
			var stringComparer = StringComparer.Create(_uiCulture.GetUiCulture(), false);

			var availableGroups = _groupingReadOnlyRepository.AvailableGroups(period, Group.PageMainId);
			var permittedGroups = availableGroups.Where(g => teamIsPermitted(period.StartDate, g)).ToList();

			var allTeamIds = permittedGroups.Select(g => g.TeamId.GetValueOrDefault());
			var allTeams = _teamRepository.FindTeams(allTeamIds);

			var mainGroupPage = permittedGroups.ToLookup(t => t.PageId);
			var mainGroupPageBySites = mainGroupPage[Group.PageMainId].ToLookup(g => g.SiteId);

			var ret = new List<SiteViewModelWithTeams>();
			foreach (var siteLookUp in mainGroupPageBySites)
			{
				var siteId = siteLookUp.Key;
				var teamGroups = mainGroupPageBySites[siteId].ToList();

				if (!teamGroups.Any())
				{
					continue;
				}

				var teamIds = teamGroups.Select(x => x.GroupId);
				var teams = allTeams.Where(t => teamIds.Contains(t.Id.GetValueOrDefault())).ToList();

				var children = teams.Select(t => new TeamViewModel
				{
					Name = t.Description.Name,
					Id = t.Id.GetValueOrDefault()
				}).OrderBy(c => c.Name, stringComparer);

				ret.Add(new SiteViewModelWithTeams
				{
					Name = teams.First().Site.Description.Name,
					Id = siteId.GetValueOrDefault(),
					Children = children.ToList()
				});
			}

			return ret.ToArray();
		}

		private bool teamIsPermitted(DateOnly date, ReadOnlyGroupDetail team)
		{
			return _permissionProvider.HasOrganisationDetailPermission(
				DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailWebReport, date, team);
		}
	}

	public class ValidPeriod
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}