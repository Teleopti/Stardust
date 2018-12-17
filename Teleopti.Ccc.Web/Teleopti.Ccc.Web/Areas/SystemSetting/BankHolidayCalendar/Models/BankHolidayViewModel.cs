using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models
{
	public class BankHolidayForm
	{
		public string Name { get; set; }
		public IEnumerable<DateTime> Dates {get; set; }
	}

	public class BankHolidayViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public IEnumerable<DateTime> Dates { get; set; }
	}
}