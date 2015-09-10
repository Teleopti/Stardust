using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.SeatPlanner)]
	public class SeatPlanController : ApiController
	{

		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ISeatPlanProvider _seatPlanProvider;
		private readonly ISeatBookingReportProvider _seatBookingReportProvider;
		private readonly ISeatOccupancyProvider _seatOccupancyProvider;

		public SeatPlanController(ICommandDispatcher commandDispatcher, ILoggedOnUser loggedOnUser, ISeatPlanProvider seatPlanProvider, ISeatBookingReportProvider seatBookingReportProvider, ISeatOccupancyProvider seatOccupancyProvider)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;
			_seatPlanProvider = seatPlanProvider;
			_seatBookingReportProvider = seatBookingReportProvider;
			_seatOccupancyProvider = seatOccupancyProvider;
		}

		[HttpPost, Route("api/SeatPlanner/SeatPlan"), UnitOfWork]
		public virtual IHttpActionResult Add([FromBody]AddSeatPlanCommand command)
		{
			command.TrackedCommandInfo.OperatedPersonId = command.TrackedCommandInfo != null
				? _loggedOnUser.CurrentUser().Id.Value
				: Guid.Empty;
			
			_commandDispatcher.Execute(command);
			
			return Created(Request.RequestUri, new { });
		}

		[UnitOfWork, Route("api/SeatPlanner/SeatPlan"), HttpGet]
		public virtual SeatPlanViewModel Get(DateTime date)
		{
			return _seatPlanProvider.Get(new DateOnly(date));
		}

		[UnitOfWork, Route("api/SeatPlanner/SeatPlan"), HttpGet]
		public virtual ICollection<SeatPlanViewModel> Get(DateTime startDate, DateTime endDate)
		{
			return _seatPlanProvider.Get(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate))).ToArray();
		}

		[UnitOfWork, Route("api/SeatPlanner/SeatBookingReport"), HttpPost]
		public virtual SeatBookingReportViewModel Get([FromBody]SeatBookingReportCommand command)
		{
			return _seatBookingReportProvider.Get(command);
		}

		[UnitOfWork, Route ("api/SeatPlanner/Occupancy"), HttpGet]
		public virtual ICollection<OccupancyViewModel> Get (Guid seatId, DateTime date)
		{
			return _seatOccupancyProvider.Get (seatId, new DateOnly(date)).ToArray();
		}
	}
}