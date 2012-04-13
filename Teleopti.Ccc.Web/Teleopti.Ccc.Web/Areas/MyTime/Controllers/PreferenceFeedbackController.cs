using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.StandardPreferences)]
	public class PreferenceFeedbackController : AsyncController
	{
		private readonly IPreferenceViewModelFactory _viewModelFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public PreferenceFeedbackController(IPreferenceViewModelFactory viewModelFactory, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_viewModelFactory = viewModelFactory;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		[UnitOfWorkAction]
		[HttpGet]
		[AsyncTimeout(1000 * 60 * 5)]
		public Task FeedbackAsync(DateOnly date)
		{
			AsyncManager.OutstandingOperations.Increment();
			return Task.Factory.StartNew(() =>
			                      	{
			                      		try
			                      		{
											using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			                      			{
			                      				var model = _viewModelFactory.CreateDayFeedbackViewModel(date);
			                      				AsyncManager.Parameters["model"] = model;												
			                      			}
			                      		}
			                      		catch (Exception e)
			                      		{
			                      			AsyncManager.Parameters["exception"] = e;
			                      		}
			                      		AsyncManager.OutstandingOperations.Decrement();
			                      	});
		}

		public JsonResult FeedbackCompleted(PreferenceDayFeedbackViewModel model, Exception exception)
		{
			if (exception != null)
				throw exception;
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}

}