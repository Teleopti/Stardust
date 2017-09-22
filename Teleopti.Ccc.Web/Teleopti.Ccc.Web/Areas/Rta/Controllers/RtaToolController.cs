using System.Linq;
using System.Web.Http;
using Castle.Core.Internal;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.RtaTool;

using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.All)]
	public class RtaToolController : ApiController
	{
		private readonly IRtaToolViewModelBuilder _viewModelBuilder;
		private readonly IRtaStateGroupRepository _stateGroups;
		private readonly ITeamCardReader _teamCardReader;

		public RtaToolController(IRtaToolViewModelBuilder viewModelBuilder, IRtaStateGroupRepository stateGroups, ITeamCardReader teamCardReader)
		{
			_viewModelBuilder = viewModelBuilder;
			_stateGroups = stateGroups;
			_teamCardReader = teamCardReader;
		}

		[UnitOfWork, ReadModelUnitOfWork, AnalyticsUnitOfWork,  HttpGet, Route("RtaTool/Agents/For")]
		public virtual IHttpActionResult GetAgents([FromUri] RtaToolAgentStateFilter filter)
		{
			if(filter == null)
				return Ok(_viewModelBuilder.Build());
			return Ok(_viewModelBuilder.Build(new RtaToolAgentStateFilter
				{
					SiteIds = filter.SiteIds,
					TeamIds = filter.TeamIds
				}
			));
		}

		[UnitOfWork, HttpGet, Route("RtaTool/PhoneStates/For")]
		public virtual IHttpActionResult GetPhoneStates()
		{
			var stateGroups = _stateGroups.LoadAllCompleteGraph().Where(x => !x.StateCollection.IsNullOrEmpty()).ToArray();
			
			var availableStateGroup = stateGroups.FirstOrDefault(x => x.Available);
			var defaultStateGroup = stateGroups.FirstOrDefault(x => x.DefaultStateGroup);
			var loggedOutStateGroup = stateGroups.FirstOrDefault(x => x.IsLogOutState);
			var result = new[] { availableStateGroup, defaultStateGroup, loggedOutStateGroup };
			var quasiRandom = stateGroups
				.Except(result)
				.OrderBy(x => x.Name).Take(3);
			
			return Ok(
				result
				.Where(x => x != null)
				.Concat(quasiRandom)
				.Select(x => new
				{
					Name = x.Name,
					Code = x.StateCollection.First().StateCode
				}));
		}

		[UnitOfWork, ReadModelUnitOfWork, HttpGet, Route("RtaTool/Organization/For")]
		public virtual IHttpActionResult GetOrganization()
		{
			var availableSites = _teamCardReader.Read().GroupBy(x => x.SiteId).Select(x=> new{SiteId = x.Key,SiteName = x.FirstOrDefault()?.SiteName}).OrderBy(x=>x.SiteName).ToArray();
			var availableTeams = _teamCardReader.Read().GroupBy(x => x.TeamId).Select(x => new { TeamId = x.Key, TeamName = x.FirstOrDefault()?.TeamName }).OrderBy(x => x.TeamName).ToArray(); ;
			return Ok(new
			{
				Sites = availableSites,
				Teams = availableTeams
			});
		}
	}

}