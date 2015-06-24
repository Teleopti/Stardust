using System;
using System.Collections.Generic;
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

		public SeatPlanController(ICommandDispatcher commandDispatcher, ILoggedOnUser loggedOnUser, ISeatPlanProvider seatPlanProvider)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;
			_seatPlanProvider = seatPlanProvider;
		}

		[HttpPost, Route("api/SeatPlanner/SeatPlan"), UnitOfWork]
		public virtual IHttpActionResult Add([FromBody]AddSeatPlanCommand command)
		{

			if (command.TrackedCommandInfo != null)
				command.TrackedCommandInfo.OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value;
			try
			{
				_commandDispatcher.Execute(command);
			}
			catch (TargetInvocationException e)
			{
				if (e.InnerException is ArgumentException)
					throw new HttpException(501, e.InnerException.Message);
			}
			
			return Created(Request.RequestUri, new {});
		}

		[UnitOfWork, Route("api/SeatPlanner/SeatPlan"), HttpGet]
		public virtual SeatPlanViewModel Get(DateTime date)
		{
			return _seatPlanProvider.Get(new DateOnly(date));
		}

		[UnitOfWork, Route("api/SeatPlanner/SeatPlan"), HttpGet]
		public virtual List<SeatPlanViewModel> Get(DateTime startDate, DateTime endDate)
		{
			return _seatPlanProvider.Get(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)));
		}
	}
}