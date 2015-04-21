using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.SeatPlanner)]
	public class LocationHierarchyController : ApiController
	{
		private readonly ILocationHierarchyProvider _locationHierarchyProvider;

	   public LocationHierarchyController(ILocationHierarchyProvider locationHierarchyProvider)
		{
			_locationHierarchyProvider = locationHierarchyProvider;
		}

		[UnitOfWork, Route("api/SeatPlanner/Locations"), HttpGet]
		public virtual LocationViewModel Get()
		{
			return _locationHierarchyProvider.Get ();
		}
	}
}


