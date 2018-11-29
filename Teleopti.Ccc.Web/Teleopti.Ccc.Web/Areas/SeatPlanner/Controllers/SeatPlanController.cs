using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.SeatPlanner)]
	public class SeatPlanController : ApiController
	{
		private readonly ISeatPlanProvider _seatPlanProvider;
		private readonly ISeatPlanner _seatPlanner;
		private readonly ISeatBookingReportProvider _seatBookingReportProvider;
		private readonly ISeatOccupancyProvider _seatOccupancyProvider;
		private readonly ISeatPlanPersister _seatPlanPerister;

		public SeatPlanController(ISeatPlanProvider seatPlanProvider,
									ISeatPlanner seatPlanner,
									ISeatBookingReportProvider seatBookingReportProvider,
									ISeatOccupancyProvider seatOccupancyProvider,
									ISeatPlanPersister seatPlanPerister)
		{

			_seatPlanProvider = seatPlanProvider;
			_seatPlanner = seatPlanner;
			_seatBookingReportProvider = seatBookingReportProvider;
			_seatOccupancyProvider = seatOccupancyProvider;
			_seatPlanPerister = seatPlanPerister;
		}

		[HttpPost, Route("api/SeatPlanner/SeatPlan"), UnitOfWork]
		public virtual IHttpActionResult Add([FromBody]AddSeatPlanCommand command)
		{
			var seatPlanningPeriod = new DateOnlyPeriod (new DateOnly (command.StartDate), new DateOnly (command.EndDate));
			var result = _seatPlanner.Plan(command.Locations, command.Teams, seatPlanningPeriod, command.SeatIds, command.PersonIds);

			return Created(Request.RequestUri, result );
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


		[UnitOfWork, Route("api/SeatPlanner/SeatBookingSummaryForDay"), HttpGet]
		public virtual SeatBookingSummary GetSeatBookingSummaryForDay(DateTime date)
		{
			return _seatBookingReportProvider.GetSummary(new DateOnly(date));
		}
		
		[UnitOfWork, Route("api/SeatPlanner/Occupancy"), HttpPost]
		public virtual ICollection<GroupedOccupancyViewModel> Get([FromBody]OccupancyListParameters occupancyListParameters)
		{
			return _seatOccupancyProvider.Get(occupancyListParameters.SeatIds, occupancyListParameters.DateOnly).ToArray();
		}

		[HttpDelete, Route("api/SeatPlanner/Occupancy"), UnitOfWork]
		public virtual IHttpActionResult Delete(Guid Id)
		{
			_seatPlanPerister.RemoveSeatBooking (Id);
			return Ok();
		}

	}

	public class OccupancyListParameters
	{
		public DateTime Date
		{
			set { DateOnly = new DateOnly(value); }
		}

		public DateOnly DateOnly { get; set; }
		public IList<Guid> SeatIds { get; set; }

	}

}