using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Controller
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.SystemSetting)]
	public class BankHolidayCalendarController : ApiController
	{
		private readonly IBankHolidayCalendarPersister _bankHolidayCalendarPersister;
		private readonly IBankHolidayCalendarProvider _bankHolidayCalendarProvider;
		private readonly IBankHolidayCalendarSiteProvider _bankHolidayCalendarSiteProvider;
		private readonly IBankHolidayCalendarSitePersister _bankHolidayCalendarSitePersister;

		public BankHolidayCalendarController(IBankHolidayCalendarPersister bankHolidayCalendarPersister, IBankHolidayCalendarProvider bankHolidayCalendarProvider,IBankHolidayCalendarSiteProvider bankHolidayCalendarSiteProvider,
			IBankHolidayCalendarSitePersister bankHolidayCalendarSitePersister)
		{
			_bankHolidayCalendarPersister = bankHolidayCalendarPersister;
			_bankHolidayCalendarProvider = bankHolidayCalendarProvider;
			_bankHolidayCalendarSiteProvider = bankHolidayCalendarSiteProvider;
			_bankHolidayCalendarSitePersister = bankHolidayCalendarSitePersister;
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
			return _bankHolidayCalendarSiteProvider.GetAllSettings();
		}

		[HttpGet, Route("api/SitesByCalendar/{Id}"), UnitOfWork]
		public virtual IEnumerable<Guid> GetSitesByCalendar(Guid Id)
		{
			return _bankHolidayCalendarSiteProvider.GetSitesByAssignedCalendar(Id);
		}

		[HttpPost, Route("api/SiteBankHolidayCalendars/Update"), UnitOfWork]
		public virtual bool SetCalendarsToSite([FromBody]SiteBankHolidayCalendarForm input)
		{
			return _bankHolidayCalendarSitePersister.UpdateCalendarsForSites(input.Settings);
		}
	}
}