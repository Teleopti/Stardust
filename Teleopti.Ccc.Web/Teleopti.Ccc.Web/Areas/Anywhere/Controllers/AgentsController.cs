using System;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
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
	    private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;

		public AgentsController(IPermissionProvider permissionProvider, ITeamRepository teamRepository, IPersonRepository personRepository, INow date, ICommonAgentNameProvider commonAgentNameProvider, IAgentStateReadModelReader agentStateReadModelReader)
		{
			_permissionProvider = permissionProvider;
			_teamRepository = teamRepository;
			_personRepository = personRepository;
			_date = date;
		    _commonAgentNameProvider = commonAgentNameProvider;
		    _agentStateReadModelReader = agentStateReadModelReader;
		}

		[HttpGet]
		public JsonResult GetStates(Guid teamId)
		{
			var states = _agentStateReadModelReader.LoadForTeam(teamId).Select(x => new AgentStateViewModel
			{
				PersonId = x.PersonId,
				State = x.State,
				StateStart = getNullableUtcDatetime(x.StateStart),
				Activity = x.Scheduled,
				NextActivity = x.ScheduledNext,
				NextActivityStartTime = getNullableUtcDatetime(x.NextStart),
				Alarm = x.AlarmName,
				AlarmStart = getNullableUtcDatetime(x.AlarmStart),
				AlarmColor = ColorTranslator.ToHtml(Color.FromArgb(x.Color ?? 0)),
				TimeInState = calculateTimeInState(x.StateStart)
			}).ToArray();

			return Json(states, JsonRequestBehavior.AllowGet);
		}

		private static DateTime? getNullableUtcDatetime(DateTime? dateTime)
		{
			return dateTime == null ? (DateTime?)null : DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);
		}

		private int calculateTimeInState(DateTime? dateTime)
		{
			var stateStart = getNullableUtcDatetime(dateTime);
			return stateStart.HasValue ? (int) (_date.UtcDateTime() - stateStart.Value).TotalSeconds : 0;
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
								TeamName = team.Description.Name
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