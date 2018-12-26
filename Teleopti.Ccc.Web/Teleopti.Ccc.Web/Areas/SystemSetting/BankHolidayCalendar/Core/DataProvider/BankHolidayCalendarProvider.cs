using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.Mapping;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider
{
	public class BankHolidayCalendarProvider : IBankHolidayCalendarProvider
	{
		private readonly IBankHolidayCalendarRepository _bankHolidayCalendarRepository;
		private readonly IBankHolidayModelMapper _bankHolidayModelMapper;

		public BankHolidayCalendarProvider(IBankHolidayCalendarRepository bankHolidayCalendarRepository, IBankHolidayModelMapper bankHolidayModelMapper)
		{
			_bankHolidayCalendarRepository = bankHolidayCalendarRepository;
			_bankHolidayModelMapper = bankHolidayModelMapper;
		}

		public BankHolidayCalendarViewModel Load(Guid Id)
		{
			var calendar = _bankHolidayCalendarRepository.Load(Id);

			return _bankHolidayModelMapper.Map(calendar);
		}

		public IEnumerable<BankHolidayCalendarViewModel> LoadAll()
		{
			var calendars = _bankHolidayCalendarRepository.LoadAll();

			return _bankHolidayModelMapper.Map(calendars);
		}
	}
}