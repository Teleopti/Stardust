using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Ccc.Web.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class RequestsViewModel
	{
		public IEnumerable<AbsenceTypeViewModel> AbsenceTypes { get; set; }
		public IEnumerable<AbsenceTypeViewModel> AbsenceTypesForReport { get; set; }
		public RequestPermission RequestPermission { get; set; }
		public string DatePickerFormat { get; set; }
		public IEnumerable<OvertimeTypeViewModel> OvertimeTypes { get; set; }
	}

	public class RequestViewModel
	{
		public string Id;
		public Link Link;
		public string Subject;
		public string Text;
		public string Type;
		public RequestType TypeEnum;

		public String DateTimeFrom;
		public String DateTimeTo;
		public String UpdatedOnDateTime;

		public string Status;
		public string Payload;
		public string PayloadId;
		public bool IsCreatedByUser;
		
		public bool IsFullDay;
		public string DenyReason;
		public string From;
		public string To;
		public bool IsNew;
		public bool IsPending;
		public bool IsApproved;
		public bool IsDenied;
		public bool IsNextDay;
		public int DateFromYear { get; set; }
		public int DateFromMonth { get; set; }
		public int DateFromDayOfMonth { get; set; }
		public int DateToYear { get; set; }
		public int DateToMonth { get; set; }
		public int DateToDayOfMonth { get; set; }
		public bool IsReferred { get; set; }
		public ShiftExchangeOfferRequestViewModel ExchangeOffer { get; set; }
		public string MultiplicatorDefinitionSet { get; set; }
	}

	public class ShiftExchangeOfferRequestViewModel
	{
		public string WishShiftType { get; set; }
		public DateTime ValidTo { get; set; }
		public bool IsOfferAvailable { get; set; }
	}
}