using System;

namespace Teleopti.Ccc.Web.Areas.Search.Models
{
	public class PeopleSummary
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string EmploymentNumber { get; set; }
		public Guid PersonId { get; set; }
		public string Email { get; set; }
	}
}