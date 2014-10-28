using System;
using System.Web.Mvc;
using Rhino.ServiceBus.DataStructures;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[RequestPermission]
	public class ShiftExchangeController : Controller
	{
		private readonly IShiftExchangeOfferPersister _shiftExchangeOfferPersister;
		
		public ShiftExchangeController(IShiftExchangeOfferPersister shiftExchangeOfferPersister)
		{
			_shiftExchangeOfferPersister = shiftExchangeOfferPersister;
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
			return Json(_shiftExchangeOfferPersister.Persist(form));
		}
	}

	public class ShiftExchangeOfferForm
	{
		public DateTime Date { get; set; }
		public TimeSpan? StartTime { get; set; } 
		public TimeSpan? EndTime { get; set; } 
		public DateTime OfferValidTo { get; set; }
		public bool EndTimeNextDay { get; set; }
	}
}