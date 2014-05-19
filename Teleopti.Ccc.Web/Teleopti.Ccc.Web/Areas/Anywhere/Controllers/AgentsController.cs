using System;
using System.Linq;
using System.Web.Mvc;
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

		public AgentsController(IPermissionProvider permissionProvider, ITeamRepository teamRepository, IPersonRepository personRepository, INow date, IAgentStateReader agentStateReader)
		{
			_permissionProvider = permissionProvider;
			_teamRepository = teamRepository;
			_personRepository = personRepository;
			_date = date;
			_agentStateReader = agentStateReader;
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

			return Json(_agentStateReader.GetLatestStatesForTeam(teamId),JsonRequestBehavior.AllowGet);
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
					.Select(x => new AgentViewModel {Id = x.Id.GetValueOrDefault().ToString(), Name = x.Name.ToString()});
			return Json(agents, JsonRequestBehavior.AllowGet);
		}
	}

}