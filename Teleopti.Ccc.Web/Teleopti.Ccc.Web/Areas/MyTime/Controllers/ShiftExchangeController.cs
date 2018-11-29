using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;


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

		[UnitOfWork]
		[HttpPost]
		public virtual JsonResult NewOffer(ShiftExchangeOfferForm form)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			return Json(_shiftExchangeOfferPersister.Persist(form, ShiftExchangeOfferStatus.Pending));
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult GetAbsence(DateOnly date)
		{
			return Json(_viewModelFactory.CreateViewModel(_loggedOnUser.CurrentUser().Id.GetValueOrDefault(), date.Date), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult GetAllWishShiftOptions()
		{
			
			return Json(new[]
			{
				new
				{
					Id = ShiftExchangeLookingForDay.WorkingShift.ToString(),
					Description = Resources.OptionWorkingDay,
					RequireDetails = true
				},
				new
				{
					Id = ShiftExchangeLookingForDay.DayOff.ToString(),
					Description = Resources.OptionDayOff,
					RequireDetails = false
				},
				new
				{
					Id = ShiftExchangeLookingForDay.EmptyDay.ToString(),
					Description = Resources.OptionEmptyDay,
					RequireDetails = false
				},
				new
				{
					Id = ShiftExchangeLookingForDay.DayOffOrEmptyDay.ToString(),
					Description = Resources.OptionDayOffOrEmptyDay,
					RequireDetails = false
				}
			}, JsonRequestBehavior.AllowGet);
		}
	}
}