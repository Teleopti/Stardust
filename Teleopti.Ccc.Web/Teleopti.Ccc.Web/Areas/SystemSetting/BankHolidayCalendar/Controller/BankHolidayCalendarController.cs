using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Controller
{
	public class BankHolidayCalendarController: ApiController
	{
		private readonly IBankHolidayCalendarPersister _bankHolidayCalendarPersister;

		public BankHolidayCalendarController(IBankHolidayCalendarPersister bankHolidayCalendarPersister)
		{
			_bankHolidayCalendarPersister = bankHolidayCalendarPersister;
		}

		[HttpPost, Route("api/BankHolidayCalendar/Create"), UnitOfWork]
		public virtual BankHolidayViewModel CreateBankHolidayCalendar([FromBody]BankHolidayForm input)
		{
			return _bankHolidayCalendarPersister.Persist(input);
		}
	}
}