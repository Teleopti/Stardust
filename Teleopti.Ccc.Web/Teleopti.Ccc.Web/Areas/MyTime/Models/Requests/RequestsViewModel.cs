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
		//only needed for view/edit detail view 
		//move to seperate model?
		public string RawDateFrom;
		public string RawDateTo;
		public string RawTimeFrom;
		public string RawTimeTo;
		public bool IsFullDay;
		public string DenyReason;
	}
}