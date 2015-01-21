using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class AgentsController : Controller
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly ITeamRepository _teamRepository;
		private readonly IPersonRepository _personRepository;
		private readonly INow _date;
		private readonly IAgentStateReader _agentStateReader;
		private readonly IUserTimeZone _userTimeZone;
	    private readonly ICommonAgentNameProvider _commonAgentNameProvider;

	    public AgentsController(IPermissionProvider permissionProvider, ITeamRepository teamRepository, IPersonRepository personRepository, INow date, IAgentStateReader agentStateReader, IUserTimeZone userTimeZone, ICommonAgentNameProvider commonAgentNameProvider)
		{
			_permissionProvider = permissionProvider;
			_teamRepository = teamRepository;
			_personRepository = personRepository;
			_date = date;
			_agentStateReader = agentStateReader;
			_userTimeZone = userTimeZone;
		    _commonAgentNameProvider = commonAgentNameProvider;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult GetStates(Guid teamId)
		{
			var team = _teamRepository.Get(teamId);
			var isPermitted =
				_permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview,
					_date.LocalDateOnly(), team);
			if (!isPermitted)
			{
				Response.StatusCode = 403;
				return null;
			}

			return Json(_agentStateReader.GetLatestStatesForTeam(teamId).Select(x => new AgentViewModel
			{
				PersonId = x.PersonId,
				Name = _commonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription( _personRepository.Get(x.PersonId)),
				State = x.State,
				StateStart = getNullableUtcDatetime(x.StateStart ),
				Activity = x.Activity,
				NextActivity = x.NextActivity,
				NextActivityStartTime = getNullableUtcDatetime(x.NextActivityStartTime),
				Alarm = x.Alarm,
				AlarmStart = getNullableUtcDatetime(x.AlarmStart),
				AlarmColor = x.AlarmColor
			}), JsonRequestBehavior.AllowGet);
		}

		private static DateTime? getNullableUtcDatetime(DateTime? dateTime)
		{
			return dateTime == null ? (DateTime?)null : DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);
		}


		[UnitOfWorkAction, HttpGet]
		public JsonResult ForTeam(Guid teamId)
		{
			var team = _teamRepository.Get(teamId);
			var isPermitted =
				_permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview,
					_date.LocalDateOnly(), team);
			if (!isPermitted)
			{
				Response.StatusCode = 403;
				return null;
			}
			var today = _date.LocalDateOnly();
			var agents =
				_personRepository.FindPeopleBelongTeam(team, new DateOnlyPeriod(today, today))
					.Select(
						x =>
							new AgentViewModel
							{
								PersonId = x.Id.GetValueOrDefault(),
                                Name = _commonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(_personRepository.Get(x.Id.GetValueOrDefault())),
								SiteId = team.Site.Id.ToString(),
								SiteName = team.Site.Description.Name,
								TeamId = team.Id.ToString(),
								TeamName = team.Description.Name,
								TimeZoneOffsetMinutes = _userTimeZone.TimeZone().GetUtcOffset(_date.UtcDateTime()).TotalMinutes
							});
			return Json(agents, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult  Team(Guid personId, DateTime date)
		{
			var person = _personRepository.Get(personId);
			var team = person.MyTeam(new DateOnly(date));
			return Json(team.Id.GetValueOrDefault(), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult PersonDetails(Guid personId)
		{
			return Json(new
					{
						Name = _commonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(_personRepository.Get(personId))
					},
					JsonRequestBehavior.AllowGet);
		}
	}

}