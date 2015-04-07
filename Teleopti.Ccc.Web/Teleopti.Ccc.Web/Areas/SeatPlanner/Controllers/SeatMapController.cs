using System;
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
	public class SeatMapController : ApiController
	{
		private readonly ISeatMapProvider _seatMapProvider;
		private ICommandDispatcher _commandDispatcher;
		private ILoggedOnUser _loggedOnUser;

		public SeatMapController()
		{

		}
		
		public SeatMapController(ISeatMapProvider seatMapProvider,ICommandDispatcher commandDispatcher, ILoggedOnUser loggedOnUser)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;
			_seatMapProvider = seatMapProvider;
		}
		
		[UnitOfWork, Route("api/SeatPlanner/SeatMap"), HttpGet]
		public virtual LocationViewModel Get(Guid? id)
		{
			return _seatMapProvider.Get (id);
		}


		[UnitOfWork, Route("api/SeatPlanner/SeatMap"), HttpGet]
		public virtual LocationViewModel Get()
		{
			return _seatMapProvider.Get(null);
		}
		
		
		[UnitOfWork, Route("api/SeatPlanner/SeatMap"), HttpPost]
		//RobTodo: Check Permissions
		//[AddSeatMapPermission]
		public virtual bool Save([FromBody]SaveSeatMapCommand command)
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
			return true;
		}



		//Robtodo: remove when prototype is removed
		[UnitOfWork, Route("SeatPlanner/SeatMap/GetOld"), HttpGet]
		public virtual object GetOld(Guid? id)
		{
			return _seatMapProvider.Get(id);
		}
	}
}
