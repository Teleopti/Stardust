using System;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider
{
	public interface IBankHolidayCalendarPersister
	{
		BankHolidayViewModel Persist(BankHolidayForm input);
	}
	public class BankHolidayCalendarPersister: IBankHolidayCalendarPersister
	{
		private readonly IBankHolidayCalendarRepository _bankHolidayCalendarRepository;

		public BankHolidayCalendarPersister(IBankHolidayCalendarRepository bankHolidayCalendarRepository)
		{
			_bankHolidayCalendarRepository = bankHolidayCalendarRepository;
		}

		public virtual BankHolidayViewModel Persist(BankHolidayForm input)
		{
			var bankHoliday = new Domain.SystemSettingWeb.BankHolidayCalendar.BankHolidayCalendar
			{
				Name = input.Name,
				Dates = input.Dates.ToList()
			};
			_bankHolidayCalendarRepository.Add(bankHoliday);

			return new BankHolidayViewModel
			{
				Id = bankHoliday.Id.GetValueOrDefault(),
				Dates = bankHoliday.Dates,
				Name = bankHoliday.Name
			};
		}
	}
}