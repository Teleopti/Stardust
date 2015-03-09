using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.SeatPlanner)]
	public class SeatMapController : ApiController
	{
		private readonly ISeatMapProvider _seatMapProvider;

		public SeatMapController(ISeatMapProvider seatMapProvider)
		{
			_seatMapProvider = seatMapProvider;
		}

		[UnitOfWork, Route("SeatPlanner/SeatMap/Get"), HttpGet]
		public virtual object Get(Guid? id)
		{
			return _seatMapProvider.Get (id);
		}
	}
}
