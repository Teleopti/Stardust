using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeRequestForm
	{
		public IList<DateOnly> Dates { get; set; }

		public Guid PersonToId { get; set; }

		public Guid? ShiftExchangeOfferId { get; set; }

		[StringLength(80, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "TheNameIsTooLong")]
		public string Subject { get; set; }

		[StringLength(2000, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MessageTooLong")]
		public string Message { get; set; }
	}

	public class ShiftTradeMultiSchedulesForm
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public Guid PersonToId { get; set; }
	}
}