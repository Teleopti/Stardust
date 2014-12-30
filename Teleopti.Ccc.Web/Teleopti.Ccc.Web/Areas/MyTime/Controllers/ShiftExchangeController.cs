using System;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[RequestPermission]
	public class ShiftExchangeController : Controller
	{
		private readonly IShiftExchangeOfferPersister _shiftExchangeOfferPersister;
		private readonly ILoggedOnUser _loggedOnUser;

		private readonly IPersonScheduleViewModelFactory _viewModelFactory;
		
		public ShiftExchangeController(IShiftExchangeOfferPersister shiftExchangeOfferPersister, ILoggedOnUser loggedOnUser, IPersonScheduleViewModelFactory viewModelFactory)
		{
			_shiftExchangeOfferPersister = shiftExchangeOfferPersister;
			_loggedOnUser = loggedOnUser;
			_viewModelFactory = viewModelFactory;
		}

		[UnitOfWorkAction]
		[HttpPost]
		public JsonResult NewOffer(ShiftExchangeOfferForm form)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			return Json(_shiftExchangeOfferPersister.Persist(form, ShiftExchangeOfferStatus.Pending));
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult GetAbsence(DateOnly date)
		{
			return Json(_viewModelFactory.CreateViewModel((Guid)_loggedOnUser.CurrentUser().Id, date), JsonRequestBehavior.AllowGet);
		}
	}
}