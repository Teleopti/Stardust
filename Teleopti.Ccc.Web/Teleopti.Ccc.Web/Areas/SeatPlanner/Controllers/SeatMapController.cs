using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.SeatPlanner)]
	public class SeatMapController : ApiController
	{
		private readonly ISeatMapProvider _seatMapProvider;
		private readonly ISeatMapPersister _seatMapPersister;

		public SeatMapController()
		{}

		public SeatMapController(ISeatMapProvider seatMapProvider, ISeatMapPersister seatMapPersister)
		{
			_seatMapPersister = seatMapPersister;
			_seatMapProvider = seatMapProvider;
		}

		[UnitOfWork, Route("api/SeatPlanner/SeatMap"), HttpGet]
		public virtual LocationViewModel Get(Guid? id)
		{
			return _seatMapProvider.Get(id);
		}


		[UnitOfWork, Route("api/SeatPlanner/SeatMap"), HttpGet]
		public virtual LocationViewModel Get()
		{
			return _seatMapProvider.Get(null);
		}

		[UnitOfWork, Route("api/SeatPlanner/SeatMap"), HttpGet]
		public virtual LocationViewModel Get(DateTime date)
		{
			return _seatMapProvider.Get(null, new DateOnly(date));
		}


		[UnitOfWork, Route("api/SeatPlanner/SeatMap"), HttpGet]
		public virtual LocationViewModel Get(Guid id, DateTime date)
		{
			return _seatMapProvider.Get(id, new DateOnly(date));
		}

		[UnitOfWork, Route("api/SeatPlanner/SeatMap"), HttpPost]
		public virtual bool Save([FromBody]SaveSeatMapCommand command)
		{
			_seatMapPersister.Save(command);
			
			return true;
		}
	}
}
