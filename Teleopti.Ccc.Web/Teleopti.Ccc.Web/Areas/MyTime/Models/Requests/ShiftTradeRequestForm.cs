using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeRequestForm
	{
		public DateOnly Date { get; set; }
		public Guid PersonToId { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }
	}
}