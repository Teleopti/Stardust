﻿using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Ccc.Web.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class RequestsViewModel
	{
		public IEnumerable<AbsenceTypeViewModel> AbsenceTypes { get; set; }
		public RequestPermission RequestPermission { get; set; }
	}

	public class RequestViewModel
	{
		public string Id;
		public Link Link;
		public string Subject;
		public string Text;
		public string Type;
		public RequestType TypeEnum;
		public string Dates;
		public string UpdatedOn;
		public string Status;
		public string Payload;
		public bool IsCreatedByUser;
		public string RawDateFrom;
		public string RawDateTo;
		public string RawTimeFrom;
		public string RawTimeTo;
		public bool IsFullDay;
		public string DenyReason;
		public string From;
		public string To;
		public bool IsNew;
		public bool IsPending;
		public bool IsApproved;
	}
}