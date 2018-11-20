using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.Configuration;
using Teleopti.Wfm.Adherence.Tool;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.All)]
	public class RtaToolController : ApiController
	{
		private readonly RtaToolViewModelBuilderFromAgentState _viewModelBuilder;
		private readonly IRtaStateGroupRepository _stateGroups;
		private readonly ITeamCardReader _teamCardReader;

		public RtaToolController(RtaToolViewModelBuilderFromAgentState viewModelBuilder, IRtaStateGroupRepository stateGroups, ITeamCardReader teamCardReader)
		{
			_viewModelBuilder = viewModelBuilder;
			_stateGroups = stateGroups;
			_teamCardReader = teamCardReader;
		}

		[UnitOfWork, ReadModelUnitOfWork, AnalyticsUnitOfWork, HttpGet, Route("api/RtaTool/Agents")]
		public virtual IHttpActionResult Agents([FromUri] RtaToolAgentStateFilter filter)
		{
			if (filter == null)
				return Ok(_viewModelBuilder.Build());
			return Ok(_viewModelBuilder.Build(new RtaToolAgentStateFilter
				{
					SiteIds = filter.SiteIds,
					TeamIds = filter.TeamIds
				}
			));
		}

		[UnitOfWork, HttpGet, Route("api/RtaTool/PhoneStates")]
		public virtual IHttpActionResult PhoneStates()
		{
			var stateGroups = _stateGroups.LoadAllCompleteGraph()
				.Where(x => !x.StateCollection.IsNullOrEmpty())
				.Randomize()
				.ToArray();

			var available = stateGroups.FirstOrDefault(x => x.Available);
			var @default = stateGroups.FirstOrDefault(x => x.DefaultStateGroup);
			var loggedOut = stateGroups.FirstOrDefault(x => x.IsLogOutState);
			var oneOfEach = new[]
			{
				available,
				@default,
				loggedOut
			}.Where(x => x != null);

			return Ok(
				oneOfEach
					.Concat(stateGroups)
					.Take(6)
					.Select(x => new
					{
						Name = x.Name,
						Code = x.StateCollection.First().StateCode
					}));
		}

		[UnitOfWork, ReadModelUnitOfWork, HttpGet, Route("api/RtaTool/Organization")]
		public virtual IHttpActionResult Organization()
		{
			var availableSites = _teamCardReader.Read().GroupBy(x => x.SiteId).Select(x => new {SiteId = x.Key, SiteName = x.FirstOrDefault()?.SiteName}).OrderBy(x => x.SiteName).ToArray();
			var availableTeams = _teamCardReader.Read().GroupBy(x => x.TeamId).Select(x => new {TeamId = x.Key, TeamName = x.FirstOrDefault()?.TeamName}).OrderBy(x => x.TeamName).ToArray();
			return Ok(new
			{
				Sites = availableSites,
				Teams = availableTeams
			});
		}
	}
}