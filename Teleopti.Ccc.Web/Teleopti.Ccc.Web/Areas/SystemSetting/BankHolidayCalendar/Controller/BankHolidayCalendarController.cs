using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Controller
{
	public class BankHolidayCalendarController : ApiController
	{
		private readonly IBankHolidayCalendarPersister _bankHolidayCalendarPersister;
		private readonly IBankHolidayCalendarProvider _bankHolidayCalendarProvider;
		private readonly ISiteBankHolidayCalendarsProvider _siteBankHolidayCalendarsProvider;

		public BankHolidayCalendarController(IBankHolidayCalendarPersister bankHolidayCalendarPersister, IBankHolidayCalendarProvider bankHolidayCalendarProvider, ISiteBankHolidayCalendarsProvider siteBankHolidayCalendarsProvider)
		{
			_bankHolidayCalendarPersister = bankHolidayCalendarPersister;
			_bankHolidayCalendarProvider = bankHolidayCalendarProvider;
			_siteBankHolidayCalendarsProvider = siteBankHolidayCalendarsProvider;
		}

		[HttpGet, Route("api/BankHolidayCalendars"), UnitOfWork]
		public virtual IEnumerable<BankHolidayCalendarViewModel> LoadBankHolidayCalendars()
		{
			return _bankHolidayCalendarProvider.LoadAll();
		}

		[HttpGet, Route("api/BankHolidayCalendars/{Id}"), UnitOfWork]
		public virtual BankHolidayCalendarViewModel LoadBankHolidayCalendarById(Guid Id)
		{
			return _bankHolidayCalendarProvider.Load(Id);
		}

		[HttpPost, Route("api/BankHolidayCalendars/Save"), UnitOfWork]
		public virtual BankHolidayCalendarViewModel SaveBankHolidayCalendar([FromBody]BankHolidayCalendarForm input)
		{
			return _bankHolidayCalendarPersister.Persist(input);
		}

		[HttpDelete, Route("api/BankHolidayCalendars/{Id}"), UnitOfWork]
		public virtual bool DeleteBankHolidayCalendarById(Guid Id)
		{
			bool result = _bankHolidayCalendarPersister.Delete(Id);
			return result;
		}

		[HttpGet, Route("api/SiteBankHolidayCalendars"), UnitOfWork]
		public virtual IEnumerable<SiteBankHolidayCalendarsViewModel> GetAllSiteBankHolidayCalendars()
		{
			return _siteBankHolidayCalendarsProvider.GetAllSettings();
		}

		[HttpGet, Route("api/SitesByCalendar/{Id}"), UnitOfWork]
		public virtual IEnumerable<Guid> GetSitesByCalendar(Guid Id)
		{
			return _siteBankHolidayCalendarsProvider.GetSitesByAssignedCalendar(Id);
		}

		[HttpPost, Route("api/SiteBankHolidayCalendars/Update"), UnitOfWork]
		public virtual bool SetCalendarsToSite([FromBody]SiteBankHolidayCalendarForm input)
		{
			_siteBankHolidayCalendarsProvider.UpdateCalendarsForSites(input.Settings);
			return true;
		}
	}
}