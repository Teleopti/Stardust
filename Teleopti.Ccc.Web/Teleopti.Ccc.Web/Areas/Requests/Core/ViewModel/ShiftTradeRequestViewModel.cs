using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel
{
	public class ShiftTradeRequestViewModel : RequestViewModel
	{
		public string PersonTo { get; set; }
		public Guid PersonIdTo { get; set; }
		public string PersonToTeam { get; set; }
		public string PersonToTimeZone { get; set; }
		public IEnumerable<string> BrokenRules { get; set; }
		public IEnumerable<ShiftTradeDayViewModel> ShiftTradeDays { get; set; }
	}

	public class ShiftTradeDayViewModel
	{
		public DateOnly Date { get; set; }
		public ShiftTradeScheduleDayDetailViewModel FromScheduleDayDetail { get; set; }
		public ShiftTradeScheduleDayDetailViewModel ToScheduleDayDetail { get; set; }
	}

	public class ShiftTradeScheduleDayDetailViewModel
	{
		public string Name { get; set; }
		public ShiftObjectType Type { get; set; }
		public string ShortName { get; set; }
		public string Color { get; set; }
	}

	public enum ShiftObjectType
	{
		PersonAssignment = 1,
		DayOff,
		FullDayAbsence
	}

	public class ShiftTradeRequestListViewModel : RequestListViewModel<ShiftTradeRequestViewModel>
	{
		public DateTime MinimumDateTime;
		public DateTime MaximumDateTime;

		public int FirstDayOfWeek { get; set; }
	}

}