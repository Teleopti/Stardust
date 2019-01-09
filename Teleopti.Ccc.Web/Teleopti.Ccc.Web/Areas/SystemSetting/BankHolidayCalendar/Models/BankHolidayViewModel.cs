using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models
{
	public class BankHolidayCalendarForm
	{
		public Guid? Id { get; set; }
		public string Name { get; set; }
		public IEnumerable<BankHolidayYearForm> Years { get; set; }
	}

	public class BankHolidayYearForm
	{
		public int Year { get; set; }
		public IEnumerable<BankHolidayDateForm> Dates { get; set; }
	}

	public class BankHolidayDateForm
	{
		public Guid? Id { get; set; }
		public DateOnly Date { get; set; }
		public string Description { get; set; }
		public bool IsDeleted { get; set; }
	}

	public class BankHolidayCalendarInfoViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}

	public class BankHolidayCalendarViewModel : BankHolidayCalendarInfoViewModel
	{
		public IEnumerable<BankHolidayYearViewModel> Years { get; set; }
	}

	public class BankHolidayYearViewModel
	{
		public int? Year
		{
			get
			{
				return Dates?.First().Date.Year;
			}
		}
		public IEnumerable<BankHolidayDateViewModel> Dates { get; set; }
	}

	public class BankHolidayDateViewModel
	{
		public Guid Id { get; set; }
		public DateOnly Date { get; set; }
		public string Description { get; set; }
		public bool IsDeleted { get; set; }
	}

	public class SiteBankHolidayCalendarsViewModel
	{
		public Guid Site { get; set; }
		public IEnumerable<BankHolidayCalendarInfoViewModel> Calendars { get; set; }
	}

	public class SiteBankHolidayCalendarForm
	{
		public IEnumerable<SiteBankHolidayCalendarsViewModel> Settings { get; set; }
	}
}