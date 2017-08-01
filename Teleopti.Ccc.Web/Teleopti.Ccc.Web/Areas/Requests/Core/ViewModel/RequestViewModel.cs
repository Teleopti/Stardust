using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel
{
	public class RequestViewModel
	{
		public string Subject { get; set; }
		public string Message { get; set; }
		public string AgentName { get; set; }
		public Guid PersonId { get; set; }
		public string TimeZone { get; set; }
		public Guid Id { get; set; }
		public int Seniority { get; set; }
		public DateTime PeriodStartTime { get; set; }
		public DateTime PeriodEndTime { get; set; }
		public DateTime? UpdatedTime { get; set; }
		public DateTime? CreatedTime { get; set; }
		public RequestType Type { get; set; }
		public string TypeText { get; set; }
		public RequestStatus Status { get; set; }
		public string StatusText { get; set; }
		public Description Payload { get; set; }
		public string Team { get; set; }
		public bool IsFullDay { get; set; }
		public string DenyReason { get; set; }

		public PersonAccountSummaryViewModel PersonAccountSummaryViewModel { get; set; }


	}

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
		public ShiftObjectType Type{ get; set; }
		public string ShortName { get; set; }
		public string Color { get; set; }
	}


	public enum ShiftObjectType
	{
		PersonAssignment =1,
		DayOff,
		FullDayAbsence
	}

	public class RequestListViewModel
	{
		public int TotalCount;
		public int Skip;
		public int Take;
		public IEnumerable<RequestViewModel> Requests;
		public bool IsSearchPersonCountExceeded;
		public int MaxSearchPersonCount;
	}

	public class ShiftTradeRequestListViewModel : RequestListViewModel
	{
		public DateTime MinimumDateTime;
		public DateTime MaximumDateTime;

		public int FirstDayOfWeek { get; set; }
	}


	public class PersonAccountSummaryViewModel
	{
		public IEnumerable<PersonAccountSummaryDetailViewModel> PersonAccountSummaryDetails;
	}
	
	public class PersonAccountSummaryDetailViewModel
	{
		public DateTime StartDate { get; set; }
		public string RemainingDescription { get; set; }
		public string TrackingTypeDescription { get; set; }
		public DateTime EndDate { get; set; }
	}


}



