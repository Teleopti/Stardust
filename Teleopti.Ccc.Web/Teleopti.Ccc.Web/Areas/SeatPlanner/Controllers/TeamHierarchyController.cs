using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers
{

	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.SeatPlanner)]
	public class TeamHierarchyController : ApiController
	{
		private readonly ITeamsProvider _teamsProvider;

		public TeamHierarchyController(ITeamsProvider teamsProvider)
		{
			_teamsProvider = teamsProvider;
		}

		[UnitOfWork, Route("api/SeatPlanner/Teams"), HttpGet]
		public virtual BusinessUnitWithSitesViewModel Get()
		{
			return _teamsProvider.GetTeamHierarchy();

		}

	}
}