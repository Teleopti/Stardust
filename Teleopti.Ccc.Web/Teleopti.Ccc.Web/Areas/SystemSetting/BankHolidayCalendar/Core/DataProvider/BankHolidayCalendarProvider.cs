using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.Mapping;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;
using System.Linq;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider
{
	public class BankHolidayCalendarProvider : IBankHolidayCalendarProvider
	{
		private readonly IBankHolidayCalendarRepository _bankHolidayCalendarRepository;
		private readonly IBankHolidayDateRepository _bankHolidayDateRepository;
		private readonly IBankHolidayModelMapper _bankHolidayModelMapper;

		public BankHolidayCalendarProvider(IBankHolidayCalendarRepository bankHolidayCalendarRepository, IBankHolidayDateRepository bankHolidayDateRepository, IBankHolidayModelMapper bankHolidayModelMapper)
		{
			_bankHolidayCalendarRepository = bankHolidayCalendarRepository;
			_bankHolidayDateRepository = bankHolidayDateRepository;
			_bankHolidayModelMapper = bankHolidayModelMapper;
		}

		public BankHolidayCalendarViewModel Load(Guid Id)
		{
			var calendar = _bankHolidayCalendarRepository.Load(Id);

			var dates = _bankHolidayDateRepository.LoadAll().Where(d => d.Calendar.Id.Value == Id);

			return _bankHolidayModelMapper.Map(calendar,dates);
		}

		public IEnumerable<BankHolidayCalendarViewModel> LoadAll()
		{
			var calendars = _bankHolidayCalendarRepository.LoadAll();
			var dates = _bankHolidayDateRepository.LoadAll();
			return _bankHolidayModelMapper.Map(calendars, dates);
		}
	}
}