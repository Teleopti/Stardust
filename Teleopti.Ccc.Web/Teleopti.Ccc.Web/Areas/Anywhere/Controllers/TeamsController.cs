using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class TeamsController : Controller
	{
		private readonly ISiteRepository _siteRepository;
		private readonly INumberOfAgentsInTeamReader _numberOfAgentsInTeamReader;
		
		public TeamsController(ISiteRepository siteRepository, INumberOfAgentsInTeamReader numberOfAgentsInTeamReader)
		{
			_siteRepository = siteRepository;
			_numberOfAgentsInTeamReader = numberOfAgentsInTeamReader;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult ForSite(string siteId)
		{
			var teams = _siteRepository.Get(new Guid(siteId)).TeamCollection.ToArray();
			var numberOfAgents = _numberOfAgentsInTeamReader.FetchNumberOfAgents(teams);

			var returnTeams = teams.Select(team => new TeamViewModel
			{
				Id = team.Id.Value.ToString(), 
				Name = team.Description.Name, 
				NumberOfAgents = numberOfAgents == null ? 0 : numberOfAgents[team.Id.Value]
			});
			return Json(returnTeams, JsonRequestBehavior.AllowGet);
		}
	}
}