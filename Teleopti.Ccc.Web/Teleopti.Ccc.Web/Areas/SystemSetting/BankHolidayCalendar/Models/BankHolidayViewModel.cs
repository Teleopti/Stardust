using System;
using System.Collections.Generic;
using System.Linq;

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
		public DateTime Date { get; set; }
		public string Description { get; set; }
		public bool IsDeleted { get; set; }
		public BankHolidayDateAction Action
		{
			get
			{
				if (!Id.HasValue)
					return BankHolidayDateAction.CREATE;
				else if (IsDeleted)
					return BankHolidayDateAction.DELETE;
				else
					return BankHolidayDateAction.UPDATE;
			}
		}
	}

	public class BankHolidayCalendarViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
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
		public DateTime Date { get; set; }
		public string Description { get; set; }
		public bool IsDeleted { get; set; }
	}
}