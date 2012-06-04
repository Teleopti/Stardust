using Teleopti.Ccc.Web.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class RequestsViewModel
	{
	}

	public class RequestViewModel
	{
		public string Id;
		public Link Link;
		public string Subject;
		public string Text;
		public string Type;
		public string Dates;
		public string UpdatedOn;
		public string Status;
		public string Payload;
		//only needed for view/edit detail view 
		//move to seperate model?
		public string RawDateFrom;
		public string RawDateTo;
		public string RawTimeFrom;
		public string RawTimeTo;
	}
}